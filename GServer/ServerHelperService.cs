using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace GS
{
    // SHelperService is the namespace defined in the protobuf
    // SHelperServiceBase is the generated base implementation of the service
    public class ServerHelperService : SHelperService.SHelperServiceBase
    {
        private readonly ServerLogic clientLogic;

        public ServerHelperService(ServerLogic clientLogic)
        {
            this.clientLogic = clientLogic;
        }

        public override Task<LockDataReply> LockData(LockDataRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            clientLogic.DelayMessage();

            Console.WriteLine("--- Server ---");
            Console.WriteLine("Locking...");
            clientLogic.Lock(true);
            return Task.FromResult(new LockDataReply());
        }

        public override Task<WriteDataReply> WriteData(WriteDataRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            clientLogic.DelayMessage();

            Console.WriteLine("--- Server ---");
            Console.WriteLine("Writing...");
            clientLogic.Write(request.ObjectId, request.PartitionId, request.NewObject.Value);
            clientLogic.Unlock(true);
            return Task.FromResult(new WriteDataReply { Ok = true });
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

