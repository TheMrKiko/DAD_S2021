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

            new ServerLogic(id, hostname, port, min_d, max_d);

            while (true) ;
        }
    }
}

