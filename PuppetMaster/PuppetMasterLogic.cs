﻿using Grpc.Core;
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
        GetPartitionsReply PartitionsInfo(GetPartitionsRequest request);
        GetServersInfoReply ServersInfo(GetServersInfoRequest request);
    }
    public class PuppetMasterLogic : IPuppetMasterGUI
    {
        private GrpcChannel channel, pcschannel;
        //private PuppetMasterService.PuppetMasterServiceClient gclient;
        //private PuppetMasterService.PuppetMasterServiceClient client;
        private GServerService.GServerServiceClient gserver;
        private GCService.GCServiceClient gclient;
        private PNodeService.PNodeServiceClient pns;
        private ProcessCreationService.ProcessCreationServiceClient pcs;
        private Server server;
        private readonly PuppetMasterGUI guiWindow;
        private string nick;
        private readonly string hostname;
        private readonly string filename;
        private readonly Dictionary<string, List<string>> partitions = new Dictionary<string, List<string>>();

        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;
        private Dictionary<string, (string, GServerService.GServerServiceClient, PNodeService.PNodeServiceClient)> serverMap =
            new Dictionary<string, (string, GServerService.GServerServiceClient, PNodeService.PNodeServiceClient)>();
        private Dictionary<string, (string, GCService.GCServiceClient, PNodeService.PNodeServiceClient)> clientMap =
            new Dictionary<string, (string, GCService.GCServiceClient, PNodeService.PNodeServiceClient)>();

        public PuppetMasterLogic(PuppetMasterGUI guiWindow, string serverHostname, int serverPort, string filename)
        {
            this.hostname = serverHostname;
            this.guiWindow = guiWindow;
            this.filename = filename;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            server = new Server
            {
                Services = { PMasterService.BindService(new PuppetService(this)) },
                Ports = { new ServerPort(serverHostname, serverPort, ServerCredentials.Insecure) }
            };
            server.Start();
        }

        public void ExecuteCommands()
        {
            // Read the file and display it line by line.  
            string line; string[] split;
            System.IO.StreamReader file = new System.IO.StreamReader($@"./{this.filename}.txt");
            while ((line = file.ReadLine()) != null)
            {
                System.Console.WriteLine(line);
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
                        break;
                    case "Wait 2000":
                        break;
                    case "Freeze s1":
                        break;
                    case "Unfreeze s1":
                        break;
                    case "Crash s2":
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }

            file.Close();
        }

        public void ReplicationFactor(int r)
        {
            return;
        }

        public void Partitions(int n, string id, List<string> serverids)
        {
            lock (this)
                partitions[id] = serverids;
        }
        public GetPartitionsReply PartitionsInfo(GetPartitionsRequest request)
        {
            GetPartitionsReply req = new GetPartitionsReply();
            PartitionInf pinfo;
            foreach (string p in partitions.Keys)
            {
                pinfo = new PartitionInf { PartitionId = p };
                pinfo.ServerIds.Add(partitions[p]);
                req.Info.Add(pinfo);
            }
            return req;
        }

        public GetServersInfoReply ServersInfo(GetServersInfoRequest request)
        {
            GetServersInfoReply reqs = new GetServersInfoReply();

            foreach (string s1 in serverMap.Keys)
            {
                reqs.Info.Add(new ServerInf { Id = s1, Url = serverMap[s1].Item1 });
            }
            return reqs;

        }

        public void Server(string id, string url, int min_delay, int max_delay)
        {
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

                RegisterServersRequest reqs;

                reqs = new RegisterServersRequest();
                reqs.Info.Add(new ServerInfo { Id = id, Url = url });

                foreach (string s in serverMap.Keys)
                {
                    serverMap[s].Item3.RegisterServers(reqs);
                }

                lock (this)
                    serverMap[id] = (url, gserver, pns);

            }
            //Console.WriteLine($"Registered server {request.Nick} with URL {request.Url}");

        }

        public void Client(string username, string url, string script_file)
        {

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
                    clientMap[username] = (url, gclient, pns);

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

        public void ServerShutdown()
        {
            server.ShutdownAsync().Wait();
        }
    }
}
