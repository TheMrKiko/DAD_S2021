using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GS
{
    public class ServerLogic
    {
        private const string masterPort = "10001";
        private readonly string id;
        private readonly string hostname;
        private readonly int port;
        private readonly int min_d;
        private readonly int max_d;
        private readonly string masterHostname;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

        private GrpcChannel channel;
        private PMasterService.PMasterServiceClient pmc;

        public ServerLogic(string id, string hostname, int port, int min_d, int max_d, string masterHostname)
        {
            this.id = id;
            this.hostname = hostname;
            this.port = port;
            this.min_d = min_d;
            this.max_d = max_d;
            this.masterHostname = masterHostname;

            StartServerServer();

            RegisterInMaster();
        }

        public string Read(string objectId, string partitionId)
        {
            string value;
            Console.WriteLine("Will read");
            lock (data)
                if (!data.ContainsKey(partitionId))
                    data[partitionId] = new Dictionary<string, string>();

            lock (data[partitionId])
                if (!data[partitionId].TryGetValue(objectId, out value))
                    value = "N/A";
            Console.WriteLine("For partition " + partitionId + " and id " + objectId + ", i have " + value);
            return value;
        }

        public void WriteAsMaster(string objectId, string partitionId, string value)
        {
            List<SHelperService.SHelperServiceClient> serverClients = new List<SHelperService.SHelperServiceClient>();

            foreach (string s_id in serverList.Keys)
                if (s_id != id)
                    serverClients.Add(ConnectToServer(s_id));

            Console.WriteLine("Asking for locks.");
            Task.WaitAll(serverClients.Select(cl =>
                cl.LockDataAsync(new LockDataRequest()).ResponseAsync).ToArray()
            );
            Console.WriteLine("Locked. Asking to write");
            Task.WaitAll(serverClients.Select(cl =>
                cl.WriteDataAsync(new WriteDataRequest
                {
                    ObjectId = objectId,
                    PartitionId = partitionId,
                    NewObject = new Object { Value = value }
                }).ResponseAsync).ToArray()
            );

            Console.WriteLine("Unlocked and written in others.");
            lock (data)
                Write(objectId, partitionId, value);
        }

        public void Write(string objectId, string partitionId, string newObj)
        {
            Console.WriteLine("Starting to actually write...");
            if (!data.ContainsKey(partitionId))
                data[partitionId] = new Dictionary<string, string>();

            data[partitionId].Add(objectId, newObj);

            Console.WriteLine("Done.");
        }

        public void StartServerServer()
        {
            Server server = new Server
            {
                Services = {
                    GSService.BindService(new GServerService(this)),
                    SHelperService.BindService(new ServerHelperService(this)),
                    PNodeService.BindService(new PuppetNodeService(this))
                },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Insecure ChatServer server listening on port " + port);

            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public void RegisterInMaster()
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("Master, i'm ready for you!");
            Console.WriteLine("Waiting for some info on the network");

            channel = GrpcChannel.ForAddress($"http://{masterHostname}:{masterPort}");
            pmc = new PMasterService.PMasterServiceClient(channel);
            pmc.Register(new RegisterRequest { Id = id, Type = NodeType.Server });
        }

        public SHelperService.SHelperServiceClient ConnectToServer(string id)
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("Will connect to server " + id);

            channel = GrpcChannel.ForAddress(serverList[id]);
            return new SHelperService.SHelperServiceClient(channel);
        }

        public bool Lock()
        {
            semaphore.Wait();
            Console.WriteLine("Locked");
            return true;
        }

        public bool Unlock()
        {
            semaphore.Release();
            Console.WriteLine("Unlocked");
            return true;
        }

        public void StorePartitions(Dictionary<string, List<string>> parts)
        {
            lock (this)
                foreach (string p_id in parts.Keys)
                    this.partitionList[p_id] = parts[p_id];
        }

        public void StoreServers(Dictionary<string, string> servers)
        {
            lock (this)
                foreach (string s_id in servers.Keys)
                    this.serverList[s_id] = servers[s_id];
        }
    }
}

