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
        private readonly int port;
        private readonly Server server;
        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
        private GrpcChannel channel;
        private GSService.GSServiceClient client;
        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;

        public ClientLogic(ClientGUI guiWindow, string username, string url, string file)
        {

            Uri uri = new Uri(url);
            this.guiWindow = guiWindow;
            this.hostname = uri.Host;
            this.port = uri.Port;
            this.username = username;

            // setup the client service
            server = new Server
            {
                Services = { GCService.BindService(new GClientService(this)), PNodeService.BindService(new PuppetNodeService(this)) },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

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

            if (reply == "N/A")
            {
                ConnectToServer(server_id);
            }
            reply = client.ReadServer(request).Object.Value;
            AddMsgtoGUI($"Read: {reply}");
        }

        public void WriteObject(string part_id, string obj_id, string new_value)
        {
            bool reply;
            WriteServerRequest request = new WriteServerRequest { PartitionId = part_id, ObjectId = obj_id, NewObject= new Object { Value = new_value } };

            partitionList.TryGetValue(part_id, out List<string> servers);
            ConnectToServer(servers[0]);

            reply = client.WriteServer(request).Ok;
            AddMsgtoGUI($"Write: {reply}");
        }
       

        public void ConnectToServer(string id)
        {
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
                this.serverList.Add(id, url);
            AddMsgtoGUI(id);
        }

        public void StorePartition(string partitionId, List<string> serverIds)
        {
            lock (this)
                this.partitionList.Add(partitionId, serverIds);
            AddMsgtoGUI(partitionId);
        }

        public bool AddMsgtoGUI(string s) {
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
