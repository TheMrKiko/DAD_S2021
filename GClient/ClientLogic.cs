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
    public interface IChatClientService {
        bool AddMsgtoGUI(string s);
    }
    public class ClientLogic : IChatClientService {
        private readonly GrpcChannel channel;
        private readonly ChatServerService.ChatServerServiceClient client;
        private Server server;
        private readonly ClientGUI guiWindow;
        private string nick;
        private string hostname;
        private AsyncUnaryCall<BcastMsgReply> lastMsgCall;

        public ClientLogic(ClientGUI guiWindow, bool sec, string serverHostname, int serverPort, 
                             string clientHostname) {
            this.hostname = clientHostname;
            this.guiWindow = guiWindow;
            // setup the client side

                AppContext.SetSwitch(
                    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                channel = GrpcChannel.ForAddress("http://" + serverHostname + ":" + serverPort.ToString());
   
            client = new ChatServerService.ChatServerServiceClient(channel);
        }

        public bool AddMsgtoGUI(string s) {
            this.guiWindow.BeginInvoke(new DelAddMsg(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
        }

        public List<string> Register(string nick, string port) {
            this.nick = nick;
            // setup the client service
            server = new Server
            {
                Services = { ChatClientService.BindService(new ClientService(this)) },
                Ports = { new ServerPort(hostname, Int32.Parse(port), ServerCredentials.Insecure) }
            };
            server.Start();
            ChatClientRegisterReply reply = client.Register(new ChatClientRegisterRequest
            {
                Nick = nick,
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
                Nick = this.nick,
                Msg = m
            });
        }

        public void ServerShutdown() {
            server.ShutdownAsync().Wait();
        }
    }
}
