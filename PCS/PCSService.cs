using Grpc.Core;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PCS
{
    // ProcessCreationService is the namespace defined in the protobuf
    // ProcessCreationServiceBase is the generated base implementation of the service
    public class PCSService : ProcessCreationService.ProcessCreationServiceBase
    {
        public PCSService()
        {
        }

        public override Task<CreateClientReply> CreateClient(CreateClientRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- PCS ---");
            Console.WriteLine("Master ordered to " + context.Method);
            return Task.FromResult(NewClient(request));
        }

        public override Task<CreateServerReply> CreateServer(CreateServerRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- PCS ---");
            Console.WriteLine("Master ordered to " + context.Method);
            return Task.FromResult(NewServer(request));
        }

        private CreateClientReply NewClient(CreateClientRequest request)
        {
            Process p = new Process();
            bool r;
            const string filename = "../../../../GClient/bin/debug/netcoreapp3.1/GClient";
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $" /c start {filename} {request.Username} {request.Url} {request.ScriptFile}";
            r = p.Start();

            Console.WriteLine("Process started.");
            return new CreateClientReply { Ok = r };
        }

        private CreateServerReply NewServer(CreateServerRequest request)
        {
            Process p = new Process();
            bool r;
            const string filename = "../../../../GServer/bin/debug/netcoreapp3.1/GServer";
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $" /c start {filename} {request.Id} {request.Url} {request.MinDelay} {request.MaxDelay}";
            r = p.Start();

            Console.WriteLine("Process started.");
            return new CreateServerReply { Ok = r };
        }
    }
}

