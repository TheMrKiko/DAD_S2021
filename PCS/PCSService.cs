using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PCS
{
    // ProcessCreationService is the namespace defined in the protobuf
    // ProcessCreationServiceBase is the generated base implementation of the service
    public class PCSService : ProcessCreationService.ProcessCreationServiceBase
    {
        private readonly string masterHostname;
        private readonly List<Process> processList = new List<Process>();

        public PCSService(string masterHostname)
        {
            this.masterHostname = masterHostname;
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

        public override Task<KillReply> Kill(KillRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- PCS ---");
            Console.WriteLine("Master ordered to " + context.Method);
            return Task.FromResult(KillNodes(request));
        }

        private CreateClientReply NewClient(CreateClientRequest request)
        {
            Process p = new Process();
            bool r;
            const string filename = "../../../../GClient/bin/debug/netcoreapp3.1/GClient";
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $" /c start \"Client {request.Username}\" {filename} {masterHostname} {request.Username} {request.Url} {request.ScriptFile}";
            r = p.Start();
            processList.Add(p);

            Console.WriteLine("Process started.");
            return new CreateClientReply { Ok = r };
        }

        private CreateServerReply NewServer(CreateServerRequest request)
        {
            Process p = new Process();
            bool r;
            const string filename = "../../../../GServer/bin/debug/netcoreapp3.1/GServer";
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $" /c start \"Server {request.Id}\" {filename} {masterHostname} {request.Id} {request.Url} {request.MinDelay} {request.MaxDelay}";
            r = p.Start();
            processList.Add(p);

            Console.WriteLine("Process started.");
            return new CreateServerReply { Ok = r };
        }
        private KillReply KillNodes(KillRequest request)
        {
            foreach (Process p in processList)
            {
                p.Kill(true);
            }

            return new KillReply();
        }
    }
}

