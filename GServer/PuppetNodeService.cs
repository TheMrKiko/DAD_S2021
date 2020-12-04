using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GS
{
    // PNodeService is the namespace defined in the protobuf
    // PNodeServiceBase is the generated base implementation of the service
    public class PuppetNodeService : PNodeService.PNodeServiceBase
    {
        private readonly ServerLogic serverLogic;

        public PuppetNodeService(ServerLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<RegisterPartitionsReply> RegisterPartitions(RegisterPartitionsRequest request, ServerCallContext context)
        {
            serverLogic.DelayMessage();
            Console.WriteLine();

            Console.WriteLine($"<Master> RegisterPartitions {request.Info}");

            Dictionary<string, List<string>> parts = new Dictionary<string, List<string>>();
            foreach (PartitionInfo partition in request.Info)
                parts.Add(partition.PartitionId, new List<string>(partition.ServerIds.ToList()));

            this.serverLogic.StorePartitions(parts);
            return Task.FromResult(new RegisterPartitionsReply());
        }

        public override Task<RegisterServersReply> RegisterServers(RegisterServersRequest request, ServerCallContext context)
        {
            serverLogic.DelayMessage();
            Console.WriteLine();

            Console.WriteLine($"<Master> RegisterServers {request.Info}");

            Dictionary<string, string> servers = new Dictionary<string, string>();
            foreach (ServerInfo server in request.Info)
                servers.Add(server.Id, server.Url);

            this.serverLogic.StoreServers(servers);
            return Task.FromResult(new RegisterServersReply());
        }

        public override Task<StatusReply> Status(StatusRequest request, ServerCallContext context)
        {
            serverLogic.DelayMessage();
            Console.WriteLine();

            Console.WriteLine($"<Master> Status");

            this.serverLogic.Status();
            return Task.FromResult(new StatusReply());
        }
    }
}

