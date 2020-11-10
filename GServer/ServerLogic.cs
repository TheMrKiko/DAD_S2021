using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;

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

        private readonly Dictionary<string, string> data = new Dictionary<string, string>();
        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();

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

        public string Read(string id)
        {
            string value;
            lock (this)
                if (!data.TryGetValue(id, out value))
                    value = "N/A";
            Console.WriteLine("I only got " + value);
            return value;
        }

        public void Write(string objectId, string partitionId, string newObj)
        {
            lock (this)
            {
                data[objectId] = newObj;
            }
            Console.WriteLine("Done.");
        }

        public void StartServerServer()
        {
            Server server = new Server
            {
                Services = { GSService.BindService(new GServerService(this)), PNodeService.BindService(new PuppetNodeService(this)) },
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

