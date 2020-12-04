﻿using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GC
{
    // PNodeService is the namespace defined in the protobuf
    // PNodeServiceBase is the generated base implementation of the service
    public class PuppetNodeService : PNodeService.PNodeServiceBase
    {
        private readonly ClientLogic clientLogic;

        public PuppetNodeService(ClientLogic clientLogic)
        {
            this.clientLogic = clientLogic;
        }

        public override Task<RegisterPartitionsReply> RegisterPartitions(RegisterPartitionsRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine($"<Master> RegisterPartitions {request.Info}");

            Dictionary<string, List<string>> parts = new Dictionary<string, List<string>>();
            foreach (PartitionInfo partition in request.Info)
                parts.Add(partition.PartitionId, new List<string>(partition.ServerIds.ToList()));

            this.clientLogic.StorePartitions(parts);
            return Task.FromResult(new RegisterPartitionsReply());
        }

        public override Task<RegisterServersReply> RegisterServers(RegisterServersRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine($"<Master> RegisterServers {request.Info}");

            Dictionary<string, string> servers = new Dictionary<string, string>();
            foreach (ServerInfo server in request.Info)
                servers.Add(server.Id, server.Url);

            this.clientLogic.StoreServers(servers);
            return Task.FromResult(new RegisterServersReply());
        }

        public override Task<StatusReply> Status(StatusRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine($"<Master> Status");

            this.clientLogic.Status();
            return Task.FromResult(new StatusReply());
        }





        /*public BcastMsgReply Bcast(BcastMsgRequest request) {
            // random wait to simulate slow msg broadcast: Thread.Sleep(5000);
            Console.WriteLine("msg arrived. lazy server waiting for server admin to press key.");
            Console.ReadKey();
            lock (this) {
                foreach (string nick in clientMap.Keys) {
                    if (nick != request.Nick) {
                        try {
                            clientMap[nick].RecvMsg(new RecvMsgRequest
                            {
                                Msg = request.Nick + ": " + request.Msg 
                            });
                        } catch (Exception e) {
                            Console.WriteLine(e.Message);
                            clientMap.Remove(nick);
                        }
                    }
                }
            }
            Console.WriteLine($"Broadcast message {request.Msg} from {request.Nick}");
            return new BcastMsgReply
            {
                Ok = true
            };
        }

        public ChatClientRegisterReply Reg(ChatClientRegisterRequest request) {
                channel = GrpcChannel.ForAddress(request.Url);
                ChatClientService.ChatClientServiceClient client =
                    new ChatClientService.ChatClientServiceClient(channel);
            lock (this) {
                clientMap.Add(request.Nick, client);
            }
            Console.WriteLine($"Registered client {request.Nick} with URL {request.Url}");
            ChatClientRegisterReply reply = new ChatClientRegisterReply();
            lock (this) {
                foreach (string nick in clientMap.Keys) {
                    reply.Users.Add(new User { Nick = nick });
                }
            }
            return reply;
        }*/
    }
}

