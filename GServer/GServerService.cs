using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GS
{
    // GSService is the namespace defined in the protobuf
    // GSServiceBase is the generated base implementation of the service
    public class GServerService : GSService.GSServiceBase
    {
        private GrpcChannel channel;
        private PMasterService.PMasterServiceClient pmc;
        //private Dictionary<string, ChatClientService.ChatClientServiceClient> clientMap =
        //  new Dictionary<string, ChatClientService.ChatClientServiceClient>();

        private Dictionary<string, string> data = new Dictionary<string, string>();
        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();

        public GServerService()
        {
        }

        public void RegisterInMaster(string id)
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("Master, i'm ready for you!");
            Console.WriteLine("Waiting for some info on the network");

            string local = "localhost";
            channel = GrpcChannel.ForAddress($"http://{local}:10001");
            pmc = new PMasterService.PMasterServiceClient(channel);
            pmc.Register(new RegisterRequest { Id = id, Type = NodeType.Server });
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

        private ReadServerReply Read(ReadServerRequest request)
        {
            string id = request.ObjectId;
            if (!data.TryGetValue(id, out string value))
                value = "N/A";
            Console.WriteLine("I only got " + value);
            return new ReadServerReply { Object = new Object { Value = value } };
        }

        private WriteServerReply Write(WriteServerRequest request)
        {
            lock (this)
            {
                data[request.ObjectId] = request.NewObject.Value;
            }
            Console.WriteLine("Done.");
            return new WriteServerReply { Ok = true };
        }

        public void StoreServer(string id, string url)
        {
            lock (this)
                this.serverList[id] = url;
        }

        public void StorePartition(string partitionId, List<string> serverIds)
        {
            lock (this)
                this.partitionList[partitionId] = serverIds;
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

