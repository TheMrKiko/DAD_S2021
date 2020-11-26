using Grpc.Core;
using System;

namespace PCS
{
    class Program
    {

        public static void Main(string[] args)
        {
            const int port = 10000;
            const string hostname = "localhost";

            string masterHostname = args[0];

            Server server = new Server
            {
                Services = { ProcessCreationService.BindService(new PCSService(masterHostname)) },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Insecure PCS server listening on port " + port);
            
            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true) ;
        }
    }
}

