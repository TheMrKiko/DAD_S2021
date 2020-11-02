using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading;

namespace PCS
{
    class Program
    {

        public static void Main(string[] args)
        {
            const int port = 10000;
            const string hostname = "localhost";
            string startupMessage;
            ServerPort serverPort;

            serverPort = new ServerPort(hostname, port, ServerCredentials.Insecure);
            startupMessage = "Insecure ChatServer server listening on port " + port;

            Server server = new Server
            {
                Services = { ProcessCreationService.BindService(new PCSService()) },
                Ports = { serverPort }
            };

            server.Start();

            Console.WriteLine(startupMessage);
            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true);
        }
    }
}

