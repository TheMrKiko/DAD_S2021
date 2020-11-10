using Grpc.Core;
using System;
using System.IO;
using System.Security;
using System.Threading;

namespace GS
{
    class Program
    {

        public static void Main(string[] args)
        {
            const string hostname = "localhost";

            string id = args[0];
            string url = args[1];
            int min_d = int.Parse(args[2]);
            int max_d = int.Parse(args[3]);

            int port = new Uri(url).Port;

            GServerService service = new GServerService();

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Server server = new Server
            {
                Services = { GSService.BindService(service), PNodeService.BindService(new PuppetNodeService(service)) },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();
            service.RegisterInMaster(id);

            Console.WriteLine("Insecure ChatServer server listening on port " + port);
            //Configuring HTTP for client connections in Register method

            while (true) ;
        }
    }
}

