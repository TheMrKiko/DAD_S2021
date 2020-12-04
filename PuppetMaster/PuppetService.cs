using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetService : PMasterService.PMasterServiceBase
    {
        readonly IPuppetMasterGUI clientLogic;

        public PuppetService(IPuppetMasterGUI clientLogic)
        {
            this.clientLogic = clientLogic;
        }

        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine($"<Node> Register {request.Id} ({request.Type})");
            this.clientLogic.Register(request.Id, request.Type);
            Console.WriteLine("Some node just registed.");
            return Task.FromResult(new RegisterReply());
        }
    }
}
