using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

public delegate void DelAddMsg(string s);

namespace PuppetMaster
{
    public interface IPuppetMasterGUI
    {
        bool AddMsgtoGUI(string s);
    }
    public class PuppetMasterLogic : IPuppetMasterGUI
    {
        private GrpcChannel channel;
        //private PuppetMasterService.PuppetMasterServiceClient gclient;
        //private PuppetMasterService.PuppetMasterServiceClient client;
        private GServerService.GServerServiceClient gserver;
        private ProcessCreationService.ProcessCreationServiceClient pcs;
        private Server server;
        private readonly PuppetMasterGUI guiWindow;
        private string nick;
        private readonly string hostname;
        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;
        private Dictionary<string, GServerService.GServerServiceClient> serverMap =
            new Dictionary<string, GServerService.GServerServiceClient>();

        public PuppetMasterLogic(PuppetMasterGUI guiWindow, string serverHostname, int serverPort)
        {
            this.hostname = serverHostname;
            this.guiWindow = guiWindow;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            server = new Server
            {
                Services = { PuppetMasterService.BindService(new PuppetService(this)) },
                Ports = { new ServerPort(serverHostname, serverPort, ServerCredentials.Insecure) }
            };
            server.Start();
            
            
        }

        public bool AddMsgtoGUI(string s)
        {
            this.guiWindow.BeginInvoke(new DelAddMsg(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
        }

        public void ReplicationFactor(int r)
        {
            return;
        }

        public void Server(string id, string url, int min_delay, int max_delay)
        {
            Uri uri = new Uri(url);
            channel = GrpcChannel.ForAddress($"http://{uri.Host}:10000");
            pcs = new ProcessCreationService.ProcessCreationServiceClient(channel);
            CreateServerReply reply = pcs.CreateServer(
                new CreateServerRequest { Id = id, Url = url, MinDelay = min_delay, MaxDelay = max_delay });

            if (reply.Ok)
            {
                channel = GrpcChannel.ForAddress(url);
                gserver = new GServerService.GServerServiceClient(channel);

                lock (this)
                    serverMap.Add(url, gserver);
            }
            //Console.WriteLine($"Registered client {request.Nick} with URL {request.Url}");

        }

        public void Client(string username, string url, string script_file)
        {

        }

        /*public List<string> Register(string nick, string port)
        {
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
            });

            List<string> result = new List<string>();
            foreach (User u in reply.Users)
            {
                result.Add(u.Nick);
            }
            return result;
        }

        public async Task BcastMsg(string m)
        {
            BcastMsgReply reply;
            if (lastMsgCall != null)
            {
                reply = await lastMsgCall.ResponseAsync;
            }
            lastMsgCall = client.BcastMsgAsync(new BcastMsgRequest
            {
                Nick = this.nick,
                Msg = m
            });
        }*/

        public void ServerShutdown()
        {
            server.ShutdownAsync().Wait();
        }
    }
}
