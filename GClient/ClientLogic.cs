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
        private GrpcChannel channel;
        private GServerService.GServerServiceClient client;
        private AsyncUnaryCall<BcastMsgReply> lastMsgCall;

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
                Services = { GCService.BindService(new GClientService(this)) },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

        }

        public void ConnectToServer()
        {
            // setup the client side

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress($"http://{uri.Host}:10000");

            client = new GServerService.GServerServiceClient(channel);
        }

        /*public bool AddMsgtoGUI(string s) {
            this.guiWindow.BeginInvoke(new DelAddMsg(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
        }

        public List<string> Register(string username, string port) {
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
