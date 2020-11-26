using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace GS
{
    // PServerService is the namespace defined in the protobuf
    // PServerServiceBase is the generated base implementation of the service
    public class PuppetServerService : PServerService.PServerServiceBase
    {
        private readonly ServerLogic clientLogic;

        public PuppetServerService(ServerLogic clientLogic)
        {
            this.clientLogic = clientLogic;
        }

        public override Task<CrashReply> Crash(CrashRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            clientLogic.DelayMessage();

            Console.WriteLine("--- Server ---");
            Console.WriteLine("Crashing...");
            clientLogic.Crash();
            return Task.FromResult(new CrashReply());
        }

        public override Task<FreezeReply> Freeze(FreezeRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            clientLogic.DelayMessage();

            Console.WriteLine("--- Server ---");
            Console.WriteLine("Freezing...");
            clientLogic.Freeze();
            return Task.FromResult(new FreezeReply());
        }

        public override Task<UnfreezeReply> Unfreeze(UnfreezeRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            clientLogic.DelayMessage();

            Console.WriteLine("--- Server ---");
            Console.WriteLine("Unfreezing...");
            clientLogic.Unfreeze();
            return Task.FromResult(new UnfreezeReply());
        }
    }
}

