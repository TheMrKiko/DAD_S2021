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
        private readonly GServerService serverService;

        public PuppetNodeService(GServerService serverService)
        {
            this.serverService = serverService;
        }

        public override Task<RegisterPartitionsReply> RegisterPartitions(RegisterPartitionsRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("Master says to "+ context.Method);
            Console.WriteLine("-- As in: "+ request);

            foreach (PartitionInfo partition in request.Info)
                this.serverService.StorePartition(partition.PartitionId, new List<string>(partition.ServerIds.ToList()));

            return Task.FromResult(new RegisterPartitionsReply());
        }

        public override Task<RegisterServersReply> RegisterServers(RegisterServersRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Server ---");
            Console.WriteLine("Master says to " + context.Method);
            Console.WriteLine("-- As in: " + request);

            foreach (ServerInfo server in request.Info)
                this.serverService.StoreServer(server.Id, server.Url);

            return Task.FromResult(new RegisterServersReply());
        }

        public override Task<StatusReply> Status(StatusRequest request, ServerCallContext context)
        {
            return base.Status(request, context);
        }
    }
}

