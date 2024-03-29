﻿using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace PuppetMaster
{
    public delegate void DelSyncConfig(ConfigSteps config);

    public enum ConfigSteps
    {
        ReplicateFactor,
        Partition,
        Server,
        Commands
    }

    public interface IPuppetMasterGUI
    {
        bool SyncConfigInGUI(ConfigSteps config);
        void Register(string id, NodeType type);
    }


    public class PuppetMasterLogic : IPuppetMasterGUI
    {
        private readonly PuppetMasterGUI guiWindow;
        private readonly string masterHostname;

        private ConfigSteps configStep;
        private Server server;
        private GrpcChannel channel, pcschannel;
        private PNodeService.PNodeServiceClient pns; private ProcessCreationService.ProcessCreationServiceClient pcs;
        RegisterPartitionsRequest partitionsRequest; RegisterServersRequest serversRequest;

        private int n_init_servers = 0;
        private int n_init_clients = 0;
        private readonly Dictionary<string, List<string>> partitions = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, (string url, PNodeService.PNodeServiceClient pnc)> serverMap =
            new Dictionary<string, (string, PNodeService.PNodeServiceClient)>();
        private readonly Dictionary<string, (string url, PNodeService.PNodeServiceClient pnc)> clientMap =
            new Dictionary<string, (string, PNodeService.PNodeServiceClient)>();

        public PuppetMasterLogic(PuppetMasterGUI guiWindow, string masterHostname, int masterPort)
        {
            this.masterHostname = masterHostname;
            this.guiWindow = guiWindow;

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
                        ReplicationFactor(int.Parse(split[1]));
                        break;
                    case "Partition":
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
            SyncConfig(ConfigSteps.ReplicateFactor);
            return;
        }

        public void Partitions(int _, string id, List<string> serverids)
        {
            SyncConfig(ConfigSteps.Partition);
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
                pns = new PNodeService.PNodeServiceClient(channel);

                lock (this)
                {
                    serverMap[id] = (url, pns);
                    n_init_servers += 1;
                }

            }
        }

        public void Client(string username, string url, string script_file)
        {
            SyncConfig(ConfigSteps.Commands);
            Uri uri = new Uri(url);
            pcschannel = GrpcChannel.ForAddress($"http://{uri.Host}:10000");
            pcs = new ProcessCreationService.ProcessCreationServiceClient(pcschannel);
            CreateClientReply reply = pcs.CreateClient(
                new CreateClientRequest { Username = username, Url = url, ScriptFile = script_file });

            if (reply.Ok)
            {
                channel = GrpcChannel.ForAddress(url);
                pns = new PNodeService.PNodeServiceClient(channel);

                lock (this)
                {
                    clientMap[username] = (url, pns);
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
            try
            {
                client.Freeze(new FreezeRequest());
            }
            catch (Exception) { }
        }

        public void Unfreeze(string id)
        {
            SyncConfig(ConfigSteps.Commands);

            PServerService.PServerServiceClient client = new PServerService.PServerServiceClient(GrpcChannel.ForAddress(serverMap[id].url));
            try
            {
                client.Unfreeze(new UnfreezeRequest());
            }
            catch (Exception) { }
        }

        public void Crash(string id)
        {
            SyncConfig(ConfigSteps.Commands);

            PServerService.PServerServiceClient client = new PServerService.PServerServiceClient(GrpcChannel.ForAddress(serverMap[id].url));
            try
            {
                client.Crash(new CrashRequest());
            }
            catch (Exception) { }
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
            SyncConfigInGUI(config);
            switch (config)
            {
                case ConfigSteps.ReplicateFactor:
                    configStep = ConfigSteps.ReplicateFactor;
                    break;
                case ConfigSteps.Partition:
                    configStep = ConfigSteps.Partition;
                    break;
                case ConfigSteps.Server:
                    configStep = ConfigSteps.Server;
                    break;
                case ConfigSteps.Commands:
                    if (configStep == ConfigSteps.Server)
                    {
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

                    else if (configStep == ConfigSteps.Commands)
                    {
                        while (n_init_clients != 0) { /*await Task.Delay(100);*/ }
                        Console.WriteLine("Clients alive.");
                    }
                    configStep = ConfigSteps.Commands;
                    break;
                default:
                    break;
            }
        }

        public bool SyncConfigInGUI(ConfigSteps s)
        {
            this.guiWindow.BeginInvoke(new DelSyncConfig(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
        }
    }
}
