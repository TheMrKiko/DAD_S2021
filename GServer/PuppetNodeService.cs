using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GS
{
    // ChatServerService is the namespace defined in the protobuf
    // ChatServerServiceBase is the generated base implementation of the service
    public class PuppetNodeService : PNodeService.PNodeServiceBase
    {
        private GrpcChannel channel;

        //private Dictionary<string, ChatClientService.ChatClientServiceClient> clientMap =
        //  new Dictionary<string, ChatClientService.ChatClientServiceClient>();
        private readonly GServerService serverService;
        private Dictionary<string, string> data = new Dictionary<string, string>();

        public PuppetNodeService(GServerService serverService)
        {
            this.serverService = serverService;
        }

        public override Task<RegisterPartitionReply> RegisterPartition(RegisterPartitionRequest request, ServerCallContext context)
        {
            Console.WriteLine("Deadline: " + context.Deadline);
            Console.WriteLine("Host: " + context.Host);
            Console.WriteLine("Method: " + context.Method);
            Console.WriteLine("Peer: " + context.Peer);

            this.serverService.StorePartition(request.PartitionId, new List<string>(request.ServerIds.ToList()));
            return Task.FromResult(new RegisterPartitionReply());
        }

        public override Task<RegisterServerReply> RegisterServer(RegisterServerRequest request, ServerCallContext context)
        {
            Console.WriteLine("Deadline: " + context.Deadline);
            Console.WriteLine("Host: " + context.Host);
            Console.WriteLine("Method: " + context.Method);
            Console.WriteLine("Peer: " + context.Peer);

            this.serverService.StoreServer(request.Id, request.Url);

            return Task.FromResult(new RegisterServerReply());
        }

        public override Task<StatusReply> Status(StatusRequest request, ServerCallContext context)
        {
            return base.Status(request, context);
        }
    }
}

