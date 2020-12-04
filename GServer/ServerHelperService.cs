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

        public override Task<AnnounceMasterReply> AnnounceMaster(AnnounceMasterRequest request, ServerCallContext context)
        {
            clientLogic.DelayMessage();
            Console.WriteLine();

            Console.WriteLine($"<Server> AnnounceMaster {request.ServerId} {request.PartitionId}");
            clientLogic.AnnounceMaster(request.ServerId, request.PartitionId);
            return Task.FromResult(new AnnounceMasterReply());
        }

        public override Task<WriteDataReply> WriteData(WriteDataRequest request, ServerCallContext context)
        {
            clientLogic.DelayMessage();
            Console.WriteLine();

            Console.WriteLine($"<Server> WriteData {request.PartitionId} {request.ObjectId} {request.NewObject.Value} {request.Version}");
            Console.WriteLine("Writing...");
            clientLogic.Write(request.ObjectId, request.PartitionId, request.NewObject.Value, request.Version);
            return Task.FromResult(new WriteDataReply { Ok = true });
        }
    }
}

