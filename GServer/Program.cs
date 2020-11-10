using System;

namespace GS
{
    class Program
    {

        public static void Main(string[] args)
        {
            string masterHostname = args[0];
            string id = args[1];
            string url = args[2];
            int min_d = int.Parse(args[3]);
            int max_d = int.Parse(args[4]);

            int port = new Uri(url).Port;
            string hostname = new Uri(url).Host;

            new ServerLogic(id, hostname, port, min_d, max_d, masterHostname);

            while (true) ;
        }
    }
}

