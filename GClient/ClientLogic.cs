using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
        private readonly ClientGUI guiWindow;
        private readonly string username;
        private readonly string hostname;
        private readonly string file;
        private readonly int port;
        private readonly Server server;
        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
        private GrpcChannel channel;
        private GSService.GSServiceClient client;
        private PMasterService.PMasterServiceClient pmc;
        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;

        public ClientLogic(ClientGUI guiWindow, string username, string url, string file)
        {

            Uri uri = new Uri(url);
            this.guiWindow = guiWindow;
            this.hostname = uri.Host;
            this.port = uri.Port;
            this.username = username;
            this.file = file;

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // setup the client service
            server = new Server
            {
                Services = { GCService.BindService(new GClientService(this)), PNodeService.BindService(new PuppetNodeService(this)) },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Insecure ChatServer server listening on port " + port);

            RegisterInMaster();
        }

        public void ExecuteCommands()
        {
            // Read the file and display it line by line.  
            string line; string[] split;
            System.IO.StreamReader file = new System.IO.StreamReader(@$"../../../../GClient/bin/debug/netcoreapp3.1/{this.file}.txt");
            while ((line = file.ReadLine()) != null)
            {
                System.Console.WriteLine(line);
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
                        //listServer s1
                        break;
                    case "wait":
                        //2009
                        break;
                    case "begin-repeat":
                        //5
                        break;
                    case "end-repeat":
                        break;
                    case "listGlobal":
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }

            file.Close();
        }

        public void RegisterInMaster()
        {
            Console.WriteLine();
            Console.WriteLine("--- Client ---");
            Console.WriteLine("Master, i'm ready for you!");
            Console.WriteLine("Waiting for some info on the network");

            string local = "localhost";
            channel = GrpcChannel.ForAddress($"http://{local}:10001");
            pmc = new PMasterService.PMasterServiceClient(channel);
            pmc.Register(new RegisterRequest { Id = username, Type = NodeType.Client });
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


        public void ConnectToServer(string id)
        {
            Console.WriteLine();
            Console.WriteLine("--- Client ---");
            Console.WriteLine("Will switch to server " + id);

            // setup the client side
            if (channel != null)
                channel.Dispose();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress(serverList[id]);
            client = new GSService.GSServiceClient(channel);
        }

        public void StoreServer(string id, string url)
        {
            lock (this)
                this.serverList[id] = url;
        }

        public void StorePartition(string partitionId, List<string> serverIds)
        {
            lock (this)
                this.partitionList[partitionId] = serverIds;
        }

        public bool AddMsgtoGUI(string s)
        {
            this.guiWindow.BeginInvoke(new DelAddMsg(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
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

        public void ServerShutdown()
        {
            server.ShutdownAsync().Wait();
        }
    }
}
