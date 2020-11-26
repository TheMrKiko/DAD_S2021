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
        private readonly ManualResetEventSlim manual = new ManualResetEventSlim(true);
        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

        private Server server;
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
            CheckFreeze();

            Console.WriteLine("Will read");

            Lock();
            if (!data.ContainsKey(partitionId))
                data[partitionId] = new Dictionary<string, string>();

            if (!data[partitionId].TryGetValue(objectId, out string value))
                value = "N/A";
            Unlock();

            Console.WriteLine("For partition " + partitionId + " and id " + objectId + ", i have " + value);
            return value;
        }

        public void WriteAsMaster(string objectId, string partitionId, string value, in out string cal)
        {
            CheckFreeze();

            Dictionary<string, SHelperService.SHelperServiceClient> serverClients =
                new Dictionary<string, SHelperService.SHelperServiceClient>();

            foreach (string s_id in partitionList[partitionId])
                if (s_id != id)
                    serverClients.Add(s_id, ConnectToServer(s_id));

            Console.WriteLine("Asking for locks.");
            List<(string id, Task<LockDataReply> lockReply)> lockReplies = serverClients.Select(sk => (
                sk.Key,
                sk.Value.LockDataAsync(new LockDataRequest()).ResponseAsync
            )).ToList();

            foreach ((string id, Task<LockDataReply> resp) in lockReplies)
            {
                try
                {
                    resp.Wait();
                } catch (Exception)
                {
                    Console.WriteLine($"Warning: Server {id} might me down.");
                    if (ServerDown(id))
                        serverClients.Remove(id);
                }
            }

            Console.WriteLine("Locked. Asking to write");
            List<(string id, Task<WriteDataReply> writeReply)> writeReply = serverClients.Select(sk => (
                sk.Key,
                sk.Value.WriteDataAsync(new WriteDataRequest
                {
                    ObjectId = objectId,
                    PartitionId = partitionId,
                    NewObject = new Object { Value = value }
                }).ResponseAsync
            )).ToList();

            foreach ((string id, Task<WriteDataReply> resp) in writeReply)
            {
                try
                {
                    resp.Wait();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Warning: Server {id} might me down.");
                    //if (ServerDown(id))
                        serverClients.Remove(id);
                }
            }

            Console.WriteLine("Unlocked and written in others.");

            Lock();
            Write(objectId, partitionId, value);
            Unlock();
        }

        public void Write(string objectId, string partitionId, string newObj)
        {
            CheckFreeze();

            Console.WriteLine("Starting to actually write...");
            if (!data.ContainsKey(partitionId))
                data[partitionId] = new Dictionary<string, string>();

            data[partitionId].Add(objectId, newObj);

            Console.WriteLine("Done.");
        }

        public List<(string id, bool master)> List()
        {
            CheckFreeze();

            List<(string id, bool master)> list = new List<(string id, bool master)>();
            foreach (string p in data.Keys)
                foreach (string obj_id in data[p].Keys)
                    list.Add((obj_id, partitionList[p][0] == id));
            return list;
        }

        public void Crash()
        {
            CheckFreeze();

            Task.Run(() =>
            {
                server.KillAsync();
                Console.WriteLine("Dead.");
            });

            //System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            //process.CloseMainWindow();
        }

        public void Freeze()
        {
            manual.Reset();
            Console.WriteLine("Frozen.");
        }
        public void Unfreeze()
        {
            manual.Set();
            Console.WriteLine("Let it go!");
        }

        public void StartServerServer()
        {
            server = new Server
            {
                Services = {
                    GSService.BindService(new GServerService(this)),
                    SHelperService.BindService(new ServerHelperService(this)),
                    PNodeService.BindService(new PuppetNodeService(this)),
                    PServerService.BindService(new PuppetServerService(this))
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

        public bool Lock(bool verbose = false)
        {
            CheckFreeze();

            if (semaphore.CurrentCount == 0) Console.WriteLine("Full. Waiting...");
            semaphore.Wait();
            if (verbose) Console.WriteLine("Locked.");
            return true;
        }

        public bool Unlock(bool verbose = false)
        {
            CheckFreeze();

            semaphore.Release();
            if (verbose) Console.WriteLine("Unlocked");
            return true;
        }

        public void CheckFreeze()
        {
            if (!manual.IsSet)
                Console.WriteLine("This frozen. Will wait a bit...");
            manual.Wait();
        }

        public void StorePartitions(Dictionary<string, List<string>> parts)
        {
            CheckFreeze();

            Lock();
            foreach (string p_id in parts.Keys)
                this.partitionList[p_id] = parts[p_id];
            Unlock();
        }

        public void StoreServers(Dictionary<string, string> servers)
        {
            CheckFreeze();

            Lock();
            foreach (string s_id in servers.Keys)
                this.serverList[s_id] = servers[s_id];
            Unlock();
        }

        public void Status()
        {
            CheckFreeze();

            Console.WriteLine($"Servers: {serverList}");
            Console.WriteLine($"Partitions: {partitionList}");
            Console.WriteLine($"Data stored: {data}");
        }
    }
}

