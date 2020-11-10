using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GC
{
    // ChatServerService is the namespace defined in the protobuf
    // ChatServerServiceBase is the generated base implementation of the service
    public class PuppetNodeService : PNodeService.PNodeServiceBase
    {
        private GrpcChannel channel;
        private readonly ClientLogic clientLogic;

        public PuppetNodeService(ClientLogic clientLogic)
        {
            this.clientLogic = clientLogic;
        }

        public override Task<RegisterPartitionsReply> RegisterPartitions(RegisterPartitionsRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Client ---");
            Console.WriteLine("Master says to " + context.Method);
            Console.WriteLine("-- As in: " + request);

            foreach (PartitionInfo partition in request.Info)
                this.clientLogic.StorePartition(partition.PartitionId, new List<string>(partition.ServerIds.ToList()));
            this.clientLogic.Registed();
            return Task.FromResult(new RegisterPartitionsReply());
        }

        public override Task<RegisterServersReply> RegisterServers(RegisterServersRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Client ---");
            Console.WriteLine("Master says to " + context.Method);
            Console.WriteLine("-- As in: " + request);

            foreach (ServerInfo server in request.Info)
                this.clientLogic.StoreServer(server.Id, server.Url);
            this.clientLogic.Registed();

            return Task.FromResult(new RegisterServersReply());
        }

        public override Task<StatusReply> Status(StatusRequest request, ServerCallContext context)
        {
            return base.Status(request, context);
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

