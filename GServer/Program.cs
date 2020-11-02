﻿using Grpc.Core;
using System;
using System.IO;
using System.Security;
using System.Threading;

namespace chatServer
{
    class Program {
        
        public static void Main(string[] args) {
            const string hostname = "localhost";

            string id = args[0];
            string url = args[1];
            int min_d = int.Parse(args[2]);
            int max_d = int.Parse(args[3]);

            int port = new Uri(url).Port;

            string startupMessage;
            ServerPort serverPort;

            serverPort = new ServerPort(hostname, port, ServerCredentials.Insecure);
            startupMessage = "Insecure ChatServer server listening on port " + port;

            Server server = new Server
            {
                Services = { ChatServerService.BindService(new ServerService()) },
            Ports = { serverPort }
        };

            server.Start();

            Console.WriteLine(startupMessage);
            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch(
  "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            while (true) ;
        }
    }
}

