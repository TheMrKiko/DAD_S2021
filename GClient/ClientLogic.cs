using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public delegate void DelAddMsg(string s);

namespace GC
{
    public interface IClientGUI
    {
        bool AddMsgtoGUI(string s);
    }
    public class ClientLogic : IClientGUI
    {
        private const string masterPort = "10001";
        private readonly ClientGUI guiWindow;
        private readonly string username;
        private readonly string hostname;
        private readonly int port;
        private readonly string masterHostname;

        private Server server;
        private GrpcChannel channel;
        private GSService.GSServiceClient client;
        private PMasterService.PMasterServiceClient pmc;

        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
        private int ready = 2;

        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;

        public ClientLogic(ClientGUI guiWindow, string username, string url, string masterHostname)
        {
            this.guiWindow = guiWindow;
            this.username = username;
            this.masterHostname = masterHostname;

            Uri uri = new Uri(url);
            this.hostname = uri.Host;
            this.port = uri.Port;

            StartClientServer();

            RegisterInMaster();

            while (ready != 0) { /*await Task.Delay(100);*/ }
        }

        public void ExecuteCommands(string filename)
        {
            // Read the file and display it line by line.  
            string line; string[] split;
            System.IO.StreamReader file = new System.IO.StreamReader(@$"../../../../GClient/bin/debug/netcoreapp3.1/{filename}");
            while ((line = file.ReadLine()) != null)
            {
                split = line.Split();
                switch (split[0])
                {
                    case "write":
                        WriteObject(split[1], split[2], line.Split('"')[1]); //write p1 obj -$i "value-$i"
                        break;
                    case "read":
                        ReadObject(split[1], split[2], split[3]);
                        break;
                    case "listServer":
                        ListServer(split[1]);
                        break;
                    case "wait":
                        Delay(int.Parse(split[1]));
                        break;
                    case "begin-repeat":
                        //5
                        break;
                    case "end-repeat":
                        break;
                    case "listGlobal":
                        ListGlobal();
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
                Console.WriteLine(line);
            }

            file.Close();
        }

        public void ReadObject(string part_id, string obj_id, string server_id)
        {
            string reply;
            ReadServerRequest request = new ReadServerRequest { PartitionId = part_id, ObjectId = obj_id };
            if (client == null)
            {
                partitionList.TryGetValue(part_id, out List<string> servers);
                ConnectToServer(servers[0]);
            }
            reply = client.ReadServer(request).Object.Value;

            if (reply == "N/A" && server_id != "-1")
            {
                ConnectToServer(server_id);
                reply = client.ReadServer(request).Object.Value;
            }
            AddMsgtoGUI($"Read: {reply}");
        }

        public void WriteObject(string part_id, string obj_id, string new_value)
        {
            bool reply;
            WriteServerRequest request = new WriteServerRequest { PartitionId = part_id, ObjectId = obj_id, NewObject = new Object { Value = new_value } };

            partitionList.TryGetValue(part_id, out List<string> servers);
            ConnectToServer(servers[0]);

            reply = client.WriteServer(request).Ok;
            AddMsgtoGUI($"Write: {reply}");
        }

        public void ListServer(string id)
        {
            Console.WriteLine("-- Info for server " + id + " --");
            GSService.GSServiceClient client = new GSService.GSServiceClient(GrpcChannel.ForAddress(serverList[id]));
            foreach (ObjectInfo objectInfo in client.ListServer(new ListServerRequest()).ObjInfo)
                Console.WriteLine("Id: " + objectInfo.Id + " (is master: " + objectInfo.Master + ")");
        }

        public void ListGlobal()
        {
            foreach (string s_id in serverList.Keys)
                ListServer(s_id);
        }

        public void Delay(int ms)
        {
            Task.WaitAll(Task.Delay(ms));
        }

        public void StartClientServer()

        {
            // setup the client service
            server = new Server
            {
                Services = { GCService.BindService(new GClientService(this)), PNodeService.BindService(new PuppetNodeService(this)) },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Insecure ChatServer server listening on port " + port);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public void RegisterInMaster()
        {
            Console.WriteLine();
            Console.WriteLine("--- Client ---");
            Console.WriteLine("Master, i'm ready for you!");
            Console.WriteLine("Waiting for some info on the network");

            channel = GrpcChannel.ForAddress($"http://{masterHostname}:{masterPort}");
            pmc = new PMasterService.PMasterServiceClient(channel);
            pmc.Register(new RegisterRequest { Id = username, Type = NodeType.Client });
        }

        public void Registed()
        {
            lock (this)
                this.ready -= 1;
        }

        public void ConnectToServer(string id)
        {
            Console.WriteLine();
            Console.WriteLine("--- Client ---");
            Console.WriteLine("Will switch to server " + id);

            // setup the client side
            if (channel != null)
                channel.Dispose();

            channel = GrpcChannel.ForAddress(serverList[id]);
            client = new GSService.GSServiceClient(channel);
        }

        public void StorePartitions(Dictionary<string, List<string>> parts)
        {
            lock (this)
                foreach (string p_id in parts.Keys)
                    this.partitionList[p_id] = parts[p_id];
            Registed();
        }

        public void StoreServers(Dictionary<string, string> servers)
        {
            lock (this)
                foreach (string s_id in servers.Keys)
                    this.serverList[s_id] = servers[s_id];
            Registed();
        }

        public bool AddMsgtoGUI(string s)
        {
            this.guiWindow.BeginInvoke(new DelAddMsg(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
        }

        public void ServerShutdown()
        {
            server.ShutdownAsync().Wait();
        }

        /*public List<string> Register(string username, string port) {
            this.username = username;
            // setup the client service
            server = new Server
            {
                Services = { ChatClientService.BindService(new GClientService(this)) },
                Ports = { new ServerPort(hostname, Int32.Parse(port), ServerCredentials.Insecure) }
            };
            server.Start();
            ChatClientRegisterReply reply = client.Register(new ChatClientRegisterRequest
            {
                Nick = username,
                Url = "http://localhost:" + port
            }) ;
            
            List<string> result = new List<string>();
            foreach (User u in reply.Users) {
                result.Add(u.Nick);
            }
            return result;
        }

        public async Task BcastMsg(string m) {
            BcastMsgReply reply;
            if (lastMsgCall != null) {
                reply = await lastMsgCall.ResponseAsync;          
            }
            lastMsgCall = client.BcastMsgAsync(new BcastMsgRequest { 
                Nick = this.username,
                Msg = m
            });
        }*/
    }
}
