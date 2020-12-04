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
            Console.WriteLine($"<Client> Read p_id: {request.PartitionId} obj_id: {request.ObjectId}");
            return Task.FromResult(Read(request));
        }

        public override Task<WriteServerReply> WriteServer(WriteServerRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine($"<Client> Write p_id: {request.PartitionId} obj_id: {request.ObjectId} obj: {request.NewObject}");
            return Task.FromResult(Write(request));
        }

        public override Task<ListServerReply> ListServer(ListServerRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine($"<Client> List");
            ListServerReply reply = new ListServerReply();
            List<(string, bool)> list = clientLogic.List();
            foreach ((string id, bool master) in list)
                reply.ObjInfo.Add(new ObjectInfo { Id = id, Master = master });

            Console.WriteLine($"Listed.");
            return Task.FromResult(reply);
        }

        private ReadServerReply Read(ReadServerRequest request)
        {
            (string val, int vers) = clientLogic.Read(request.ObjectId, request.PartitionId);

            return new ReadServerReply { Object = new Object { Value = val }, Version = vers };
        }

        private WriteServerReply Write(WriteServerRequest request)
        {
            int version = clientLogic.WriteAsMaster(request.ObjectId, request.PartitionId, request.NewObject.Value);
            return new WriteServerReply { Version = version };
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

