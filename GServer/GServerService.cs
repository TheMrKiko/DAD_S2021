using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GS
{
    // GSService is the namespace defined in the protobuf
    // GSServiceBase is the generated base implementation of the service
    public class GServerService : GSService.GSServiceBase
    {
        private readonly ServerLogic clientLogic;

        public GServerService(ServerLogic clientLogic)
        {
            this.clientLogic = clientLogic;
        }

        public override Task<ReadServerReply> ReadServer(ReadServerRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("A client is " + context.Method);
            Console.WriteLine("-- As in: " + request);
            return Task.FromResult(Read(request));
        }

        public override Task<WriteServerReply> WriteServer(WriteServerRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("A client is " + context.Method);
            Console.WriteLine("-- As in: " + request);
            return Task.FromResult(Write(request));
        }

        public override Task<ListServerReply> ListServer(ListServerRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("A client is " + context.Method);
            ListServerReply reply = new ListServerReply();
            List<(string, bool)> list = clientLogic.List();
            foreach ((string id, bool master) in list)
                reply.ObjInfo.Add(new ObjectInfo { Id = id, Master = master });
            return Task.FromResult(reply);
        }

        private ReadServerReply Read(ReadServerRequest request)
        {
            (string val, int vers) value = clientLogic.Read(request.ObjectId, request.PartitionId);

            return new ReadServerReply { Object = new Object { Value = value.val } };
        }

        private WriteServerReply Write(WriteServerRequest request)
        {
            clientLogic.WriteAsMaster(request.ObjectId, request.PartitionId, request.NewObject.Value);
            return new WriteServerReply { Ok = true };
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

