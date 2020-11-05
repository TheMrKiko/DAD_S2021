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
        //bool AddMsgtoGUI(string s);
    }
    public class PuppetMasterLogic : IPuppetMasterGUI
    {
        private GrpcChannel channel, pcschannel;
        //private PuppetMasterService.PuppetMasterServiceClient gclient;
        //private PuppetMasterService.PuppetMasterServiceClient client;
        private GServerService.GServerServiceClient gserver;
        private PNodeService.PNodeServiceClient pnserver;
        private ProcessCreationService.ProcessCreationServiceClient pcs;
        private Server server;
        private readonly PuppetMasterGUI guiWindow;
        private string nick;
        private readonly string hostname;
        private readonly Dictionary<string, List<string>> partitions = new Dictionary<string, List<string>>();

        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;
        private Dictionary<string, (GServerService.GServerServiceClient, PNodeService.PNodeServiceClient)> serverMap =
            new Dictionary<string, (GServerService.GServerServiceClient, PNodeService.PNodeServiceClient)>();

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

        public void ReplicationFactor(int r)
        {
            return;
        }

        public void Partitions(int n, string id, List<string> serverids)
        {
            lock (this)
                partitions.Add(id, serverids);
        }

        public void Server(string id, string url, int min_delay, int max_delay)
        {
            Uri uri = new Uri(url);
            pcschannel = GrpcChannel.ForAddress($"http://{uri.Host}:10000");
            pcs = new ProcessCreationService.ProcessCreationServiceClient(channel);
            CreateServerReply reply = pcs.CreateServer(
                new CreateServerRequest { Id = id, Url = url, MinDelay = min_delay, MaxDelay = max_delay });

            if (reply.Ok)
            {
                channel = GrpcChannel.ForAddress(url);
                gserver = new GServerService.GServerServiceClient(channel);
                pnserver = new PNodeService.PNodeServiceClient(channel);

                RegisterPartitionRequest req;
                foreach (string p in partitions.Keys)
                {
                    req = new RegisterPartitionRequest { PartitionId = p };
                    req.ServerIds.Add(partitions[p]);
                    pnserver.RegisterPartition(req);
                }

                /*RegisterServerRequest reqs;
                foreach (string s in serverMap.Keys)
                {
                    foreach (string s1 in serverMap[s].ke)
                    {
                        reqs = new RegisterServerRequest { Id=s, Url=serverMap[s] };
                    req.ServerIds.Add(partitions[p]);
                    pnserver.RegisterServer(reqs);
                }*/

                lock (this)
                    serverMap.Add(url, (gserver, pnserver));

            }
            //Console.WriteLine($"Registered client {request.Nick} with URL {request.Url}");

        }

        public void Client(string username, string url, string script_file)
        {

        }

        /*public bool AddMsgtoGUI(string s)
        {
            this.guiWindow.BeginInvoke(new DelAddMsg(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
        }

        public List<string> Register(string nick, string port)
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
