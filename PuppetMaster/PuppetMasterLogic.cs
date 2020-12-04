using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public delegate void DelAddMsg(string s);

namespace PuppetMaster
{
    public interface IPuppetMasterGUI
    {
        //bool AddMsgtoGUI(string s);
        void Register(string id, NodeType type);
    }

    enum ConfigSteps
    {
        ReplicateFactor,
        Partition,
        Server,
        Client,
        Commands
    }

    public class PuppetMasterLogic : IPuppetMasterGUI
    {
        //private readonly PuppetMasterGUI guiWindow;
        private readonly string masterHostname;

        private ConfigSteps configStep;
        private Server server;
        private GrpcChannel channel, pcschannel;
        private GServerService.GServerServiceClient gserver; private GCService.GCServiceClient gclient;
        private PNodeService.PNodeServiceClient pns; private ProcessCreationService.ProcessCreationServiceClient pcs;
        RegisterPartitionsRequest partitionsRequest; RegisterServersRequest serversRequest;
        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;

        private int n_init_servers = 0;
        private int n_init_clients = 0;
        private readonly Dictionary<string, List<string>> partitions = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, (string url, GServerService.GServerServiceClient sc, PNodeService.PNodeServiceClient pnc)> serverMap =
            new Dictionary<string, (string, GServerService.GServerServiceClient, PNodeService.PNodeServiceClient)>();
        private readonly Dictionary<string, (string url, GCService.GCServiceClient sc, PNodeService.PNodeServiceClient pnc)> clientMap =
            new Dictionary<string, (string, GCService.GCServiceClient, PNodeService.PNodeServiceClient)>();

        public PuppetMasterLogic(PuppetMasterGUI _, string masterHostname, int masterPort)
        {
            this.masterHostname = masterHostname;
            //this.guiWindow = guiWindow;

            StartPMServer(masterHostname, masterPort);
        }

        public void ExecuteCommands(string filename)
        {
            // Read the file and display it line by line.  
            string line; string[] split;
            System.IO.StreamReader file = new System.IO.StreamReader($@"./{filename}");
            while ((line = file.ReadLine()) != null)
            {
                Console.WriteLine();
                Console.WriteLine($"Starting command: {line}");

                split = line.Split();
                switch (split[0])
                {
                    case "ReplicationFactor":
                        configStep = ConfigSteps.ReplicateFactor;
                        ReplicationFactor(int.Parse(split[1]));
                        break;
                    case "Partition":
                        configStep = ConfigSteps.Partition;
                        Partitions(int.Parse(split[1]), split[2], split.Skip(3).ToList());
                        break;
                    case "Server":
                        Server(split[1], split[2], int.Parse(split[3]), int.Parse(split[4]));
                        break;
                    case "Client":
                        Client(split[1], split[2], split[3]);
                        break;
                    case "Status":
                        Status();
                        break;
                    case "Wait":
                        Wait(int.Parse(split[1]));
                        break;
                    case "Freeze":
                        Freeze(split[1]);
                        break;
                    case "Unfreeze":
                        Unfreeze(split[1]);
                        break;
                    case "Crash":
                        Crash(split[1]);
                        break;
                    default:
                        Console.WriteLine("Not a command.");
                        break;
                }
            }

            file.Close();
        }

        public void ReplicationFactor(int _)
        {
            return;
        }

        public void Partitions(int _, string id, List<string> serverids)
        {
            lock (this)
                partitions[id] = serverids;
        }

        public void Server(string id, string url, int min_delay, int max_delay)
        {
            SyncConfig(ConfigSteps.Server);
            Uri uri = new Uri(url);
            pcschannel = GrpcChannel.ForAddress($"http://{uri.Host}:10000");
            pcs = new ProcessCreationService.ProcessCreationServiceClient(pcschannel);
            CreateServerReply reply = pcs.CreateServer(
                new CreateServerRequest { Id = id, Url = url, MinDelay = min_delay, MaxDelay = max_delay });

            if (reply.Ok)
            {
                channel = GrpcChannel.ForAddress(url);
                gserver = new GServerService.GServerServiceClient(channel);
                pns = new PNodeService.PNodeServiceClient(channel);

                lock (this)
                {
                    serverMap[id] = (url, gserver, pns);
                    n_init_servers += 1;
                }

            }
            //Console.WriteLine($"Registered server {request.Nick} with URL {request.Url}");

        }

        public void Client(string username, string url, string script_file)
        {
            SyncConfig(ConfigSteps.Client);
            Uri uri = new Uri(url);
            pcschannel = GrpcChannel.ForAddress($"http://{uri.Host}:10000");
            pcs = new ProcessCreationService.ProcessCreationServiceClient(pcschannel);
            CreateClientReply reply = pcs.CreateClient(
                new CreateClientRequest { Username = username, Url = url, ScriptFile = script_file });

            if (reply.Ok)
            {
                channel = GrpcChannel.ForAddress(url);
                gclient = new GCService.GCServiceClient(channel);
                pns = new PNodeService.PNodeServiceClient(channel);

                lock (this)
                {
                    clientMap[username] = (url, gclient, pns);
                    n_init_clients += 1;
                }
            }
        }

        public void Kill()
        {
            pcschannel = GrpcChannel.ForAddress($"http://{masterHostname}:10000");
            pcs = new ProcessCreationService.ProcessCreationServiceClient(pcschannel);
            pcs.Kill(new KillRequest());
        }

        public void Status()
        {
            SyncConfig(ConfigSteps.Commands);
            List<Task<StatusReply>> nodeReplies = new List<Task<StatusReply>>();
            StatusRequest request = new StatusRequest();
            lock (this)
            {
                foreach (string s_id in serverMap.Keys)
                    nodeReplies.Add(serverMap[s_id].pnc.StatusAsync(request).ResponseAsync);
                foreach (string c_id in clientMap.Keys)
                    nodeReplies.Add(clientMap[c_id].pnc.StatusAsync(request).ResponseAsync);
            }

            foreach (Task<StatusReply> node in nodeReplies)
                try
                {
                    node.Wait();
                }
                catch (Exception) { };

            Console.WriteLine("Told nodes to status themselves.");
        }

        public void Wait(int ms)
        {
            SyncConfig(ConfigSteps.Commands);
            Task.Delay(ms).Wait();
        }

        public void Freeze(string id)
        {
            SyncConfig(ConfigSteps.Commands);

            PServerService.PServerServiceClient client = new PServerService.PServerServiceClient(GrpcChannel.ForAddress(serverMap[id].url));
            client.Freeze(new FreezeRequest());
        }

        public void Unfreeze(string id)
        {
            SyncConfig(ConfigSteps.Commands);

            PServerService.PServerServiceClient client = new PServerService.PServerServiceClient(GrpcChannel.ForAddress(serverMap[id].url));
            client.Unfreeze(new UnfreezeRequest());
        }

        public void Crash(string id)
        {
            SyncConfig(ConfigSteps.Commands);

            PServerService.PServerServiceClient client = new PServerService.PServerServiceClient(GrpcChannel.ForAddress(serverMap[id].url));
            client.Crash(new CrashRequest());
        }

        private void StartPMServer(string serverHostname, int serverPort)
        {
            server = new Server
            {
                Services = { PMasterService.BindService(new PuppetService(this)) },
                Ports = { new ServerPort(serverHostname, serverPort, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Insecure PupperMaster server listening on port " + serverPort);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public void Register(string id, NodeType type)
        {
            switch (type)
            {
                case NodeType.Server:
                    lock (this)
                        n_init_servers -= 1;
                    break;
                case NodeType.Client:
                    Console.WriteLine($"Client {id} alive. Informing them.");

                    Task<RegisterPartitionsReply> partitionsReplies; Task<RegisterServersReply> serversReplies;

                    lock (this)
                    {
                        partitionsReplies = clientMap[id].pnc.RegisterPartitionsAsync(partitionsRequest).ResponseAsync;
                        serversReplies = clientMap[id].pnc.RegisterServersAsync(serversRequest).ResponseAsync;
                    }
                    Console.WriteLine("Informed. Waiting acks.");

                    Task.WaitAll(serversReplies, partitionsReplies);

                    lock (this)
                        n_init_clients -= 1;

                    Console.WriteLine($"Client {id} ready.");
                    break;
                default:
                    break;
            }
        }

        public void ServerShutdown()
        {
            server.ShutdownAsync().Wait();
        }

        private void SyncConfig(ConfigSteps config)
        {
            switch (config)
            {
                case ConfigSteps.ReplicateFactor:
                    break;
                case ConfigSteps.Partition:
                    break;
                case ConfigSteps.Server:
                    configStep = ConfigSteps.Server;
                    break;
                case ConfigSteps.Client:
                    if (configStep == ConfigSteps.Server)
                    {
                        configStep = ConfigSteps.Client;
                        while (n_init_servers != 0) { /*await Task.Delay(100);*/ }
                        Console.WriteLine("Servers alive. Informing them.");

                        PartitionInfo pinfo;
                        partitionsRequest = new RegisterPartitionsRequest(); serversRequest = new RegisterServersRequest();
                        List<Task<RegisterPartitionsReply>> partitionsReplies = new List<Task<RegisterPartitionsReply>>();
                        List<Task<RegisterServersReply>> serversReplies = new List<Task<RegisterServersReply>>();

                        lock (this)
                        {

                            foreach (string p in partitions.Keys)
                            {
                                pinfo = new PartitionInfo { PartitionId = p };
                                pinfo.ServerIds.Add(partitions[p]);
                                partitionsRequest.Info.Add(pinfo);
                            }

                            foreach (string s_id in serverMap.Keys)
                                serversRequest.Info.Add(new ServerInfo { Id = s_id, Url = serverMap[s_id].url });

                            foreach (string s_id in serverMap.Keys)
                            {
                                partitionsReplies.Add(serverMap[s_id].pnc.RegisterPartitionsAsync(partitionsRequest).ResponseAsync);
                                serversReplies.Add(serverMap[s_id].pnc.RegisterServersAsync(serversRequest).ResponseAsync);
                            }
                        }
                        Console.WriteLine("Informed. Waiting acks.");

                        Task.WaitAll(serversReplies.ToArray());
                        Task.WaitAll(partitionsReplies.ToArray());
                        Console.WriteLine("Servers ready.");
                    }
                    break;
                case ConfigSteps.Commands:
                    if (configStep == ConfigSteps.Client)
                    {
                        configStep = ConfigSteps.Commands;
                        while (n_init_clients != 0) { /*await Task.Delay(100);*/ }
                        Console.WriteLine("Clients alive.");
                    }
                    break;
                default:
                    break;
            }
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
    }
}
