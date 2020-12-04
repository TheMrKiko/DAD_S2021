using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public delegate void DelAddMsg(string s);

namespace GC
{
    public interface IClientGUI
    {
        bool AddMsgtoGUI(string s);
    }
    public class ClientLogic : IClientGUI
    {
        private const string masterPort = "10001";
        private readonly ClientGUI guiWindow;
        private readonly string username;
        private readonly string hostname;
        private readonly int port;
        private readonly string masterHostname;

        private Server server;
        private GrpcChannel channel;
        private string client_id;
        private GSService.GSServiceClient client;
        private PMasterService.PMasterServiceClient pmc;

        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, string> partitionMaster = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, (int vers, Dictionary<string, string> val)> data = new Dictionary<string, (int vers, Dictionary<string, string> val)>();
        private int ready = 2;

        //private AsyncUnaryCall<BcastMsgReply> lastMsgCall;

        public ClientLogic(ClientGUI guiWindow, string username, string url, string masterHostname)
        {
            this.guiWindow = guiWindow;
            this.username = username;
            this.masterHostname = masterHostname;

            Uri uri = new Uri(url);
            this.hostname = uri.Host;
            this.port = uri.Port;

            StartClientServer();

            RegisterInMaster();

            while (ready != 0) { /*await Task.Delay(100);*/ }
        }

        public void ExecuteCommands(string filename)
        {
            // Read the file and display it line by line.  
            string[] lines = System.IO.File.ReadAllLines(@$"../../../../GClient/bin/debug/netcoreapp3.1/{filename}");
            ParseLines(lines.ToList());

            void ParseLines(List<string> lines, string i = "$i")
            {
                string command; string[] split;
                bool repeat = false; int times = 0;
                List<string> commands = new List<string>();

                foreach (string line in lines)
                {
                    command = line.Replace("$i", i); split = command.Split();

                    Console.WriteLine();
                    Console.WriteLine($"Starting command: {command}");

                    if (!repeat)
                        switch (split[0])
                        {
                            case "write":
                                WriteObject(split[1], split[2], command.Split('"')[1]);
                                break;
                            case "read":
                                ReadObject(split[1], split[2], split[3]);
                                break;
                            case "listServer":
                                ListServer(split[1]);
                                break;
                            case "wait":
                                Delay(int.Parse(split[1]));
                                break;
                            case "begin-repeat":
                                times = int.Parse(split[1]);
                                commands = new List<string>();
                                repeat = true;
                                break;
                            case "listGlobal":
                                ListGlobal();
                                break;
                            default:
                                Console.WriteLine("Not a command.");
                                break;
                        }
                    else
                        switch (split[0])
                        {
                            case "end-repeat":
                                foreach (int j in Enumerable.Range(0, times))
                                    ParseLines(commands, j.ToString());
                                repeat = false;
                                break;
                            default:
                                commands.Add(command);
                                break;
                        }
                }
            }
        }

        public void ReadObject(string part_id, string obj_id, string server_id)
        {
            ReadServerReply reply = null;
            ReadServerRequest request = new ReadServerRequest { PartitionId = part_id, ObjectId = obj_id };
            AddMsgtoGUI($"<Read> {part_id} (v{(data.ContainsKey(part_id) ? data[part_id].vers : -1)}) {obj_id}");

            Queue<string> server_pos = new Queue<string>();
            server_pos.Enqueue(server_id);

            foreach (string s_id in partitionList[part_id])
                //if (s_id.notindead)
                server_pos.Enqueue(s_id);

            while (reply == null && server_pos.Count() != 0)
            {
                if (client == null || !partitionList[part_id].Contains(client_id))
                {
                    string next_serv = server_pos.Dequeue();
                    if (next_serv == "-1")
                        continue;

                    ConnectToServer(next_serv);
                }

                try
                {
                    Console.WriteLine($"Trying to read from {client_id}...");
                    reply = client.ReadServer(request);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Warning: Server {client_id} might me down.");
                    client = null;
                    //if (ServerDown(id))
                }
            }

            if (reply == null)
            {
                Console.WriteLine($"Fatal error: Everybody dead.");
                return;
            }

            (string read_val, int read_vers) = (reply.Object.Value, reply.Version);

            Console.WriteLine($"Server {client_id} said '{read_val}' (v {read_vers}).");
            int local_vers; int version; string value;

            lock (this)
            {
                local_vers = data.ContainsKey(part_id) ? data[part_id].vers : -1;
                Console.WriteLine($"Local version is {local_vers}");

                value = local_vers <= read_vers ? read_val : data[part_id].val[obj_id];
                version = Math.Max(local_vers, read_vers);

                Dictionary<string, string> newPart =
                    data.ContainsKey(part_id) ? data[part_id].val : new Dictionary<string, string>();
                newPart[obj_id] = value;

                data[part_id] = (version, newPart);
            }

            AddMsgtoGUI($"> Read '{value}' (v{version}).");
            Console.WriteLine($"Value returned is '{value}' (v {version}).");
        }

        public void WriteObject(string part_id, string obj_id, string new_value)
        {
            WriteServerReply reply = null;
            WriteServerRequest request = new WriteServerRequest { PartitionId = part_id, ObjectId = obj_id, NewObject = new Object { Value = new_value } };

            AddMsgtoGUI($"<Write> {part_id} (v{(data.ContainsKey(part_id) ? data[part_id].vers : -1)}) {obj_id} '{new_value}'");

            Queue<string> server_pos = new Queue<string>();
            server_pos.Enqueue(partitionMaster[part_id]);

            foreach (string s_id in partitionList[part_id])
                //if (s_id.notindead)
                server_pos.Enqueue(s_id);

            string next_serv = "";
            while (reply == null && server_pos.Count() != 0)
            {
                next_serv = server_pos.Dequeue();
                if (next_serv == "-1")
                    continue;

                ConnectToServer(next_serv);

                try
                {
                    Console.WriteLine($"Trying to write to {next_serv}...");
                    reply = client.WriteServer(request);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Warning: Server {next_serv} might me down.");
                    client = null;
                    //if (ServerDown(id))
                }
            }

            if (reply == null)
            {
                Console.WriteLine($"Fatal error: Everybody dead.");
                return;
            }

            (string read_master, int read_vers) = (reply.MasterId, reply.Version);
            Console.WriteLine($"Server {next_serv} said master is {read_master} (v {read_vers}).");

            if (read_master != next_serv)
            {
                Console.WriteLine($"Fatal error: Telling lies?");
                return;
            }

            lock (this)
            {
                partitionMaster[part_id] = read_master;

                Dictionary<string, string> newPart =
                    data.ContainsKey(part_id) ? data[part_id].val : new Dictionary<string, string>();
                newPart[obj_id] = new_value;

                data[part_id] = (read_vers, newPart);
            }

            AddMsgtoGUI($"> Written in {next_serv} (v{read_vers}).");
            Console.WriteLine($"Value returned is ok from {next_serv} (v {read_vers}).");
        }

        public void ListServer(string id)
        {
            try
            {
                Console.WriteLine("Info for server " + id);
                GSService.GSServiceClient client = new GSService.GSServiceClient(GrpcChannel.ForAddress(serverList[id]));
                foreach (ObjectInfo objectInfo in client.ListServer(new ListServerRequest()).ObjInfo)
                    Console.WriteLine($"> Obj: {objectInfo.Id} {(objectInfo.Master ? "is master" : "")}");
            }
            catch (Exception)
            {
                Console.WriteLine($"Warning: Server {id} might me down.");
                //if (ServerDown(id))
                //serverClients.Remove(id);
            }
        }

        public void ListGlobal()
        {
            foreach (string s_id in serverList.Keys)
                ListServer(s_id);
        }

        public void Delay(int ms)
        {
            Task.WaitAll(Task.Delay(ms));
        }

        public void StartClientServer()

        {
            // setup the client service
            server = new Server
            {
                Services = { GCService.BindService(new GClientService(this)), PNodeService.BindService(new PuppetNodeService(this)) },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Insecure Client server listening on port " + port);

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public void RegisterInMaster()
        {
            Console.WriteLine("Master, i'm ready for you!");
            Console.WriteLine("Waiting for some info on the network");

            channel = GrpcChannel.ForAddress($"http://{masterHostname}:{masterPort}");
            pmc = new PMasterService.PMasterServiceClient(channel);
            pmc.Register(new RegisterRequest { Id = username, Type = NodeType.Client });
        }

        public void Registed()
        {
            lock (this)
                this.ready -= 1;
        }

        public void ConnectToServer(string id)
        {
            Console.WriteLine("Will connect to server " + id);

            // setup the client side
            if (channel != null)
                channel.Dispose();

            channel = GrpcChannel.ForAddress(serverList[id]);
            client = new GSService.GSServiceClient(channel);
            client_id = id;
        }

        public void StorePartitions(Dictionary<string, List<string>> parts)
        {
            lock (this)
                foreach (string p_id in parts.Keys)
                {
                    this.partitionList[p_id] = parts[p_id];
                    this.partitionMaster.Add(p_id, parts[p_id][0]);
                }
            Registed();
        }

        public void StoreServers(Dictionary<string, string> servers)
        {
            lock (this)
                foreach (string s_id in servers.Keys)
                    this.serverList[s_id] = servers[s_id];
            Registed();
        }

        public void Status()
        {
            Console.WriteLine($"> Servers:");
            Console.WriteLine($"> {string.Join(", ", serverList.Keys)}");

            Console.WriteLine($"> Partitions:");
            foreach (string p_id in partitionList.Keys)
                Console.WriteLine($"> Partition {p_id} ({string.Join(", ", partitionMaster[p_id])}) is in {string.Join(", ", partitionList[p_id])}");
        }

        public bool AddMsgtoGUI(string s)
        {
            this.guiWindow.BeginInvoke(new DelAddMsg(guiWindow.AddMsgtoGUI), new object[] { s });
            return true;
        }

        public void ServerShutdown()
        {
            server.ShutdownAsync().Wait();
        }

        /*public List<string> Register(string username, string port) {
            this.username = username;
            // setup the client service
            server = new Server
            {
                Services = { ChatClientService.BindService(new GClientService(this)) },
                Ports = { new ServerPort(hostname, Int32.Parse(port), ServerCredentials.Insecure) }
            };
            server.Start();
            ChatClientRegisterReply reply = client.Register(new ChatClientRegisterRequest
            {
                Nick = username,
                Url = "http://localhost:" + port
            }) ;
            
            List<string> result = new List<string>();
            foreach (User u in reply.Users) {
                result.Add(u.Nick);
            }
            return result;
        }

        public async Task BcastMsg(string m) {
            BcastMsgReply reply;
            if (lastMsgCall != null) {
                reply = await lastMsgCall.ResponseAsync;          
            }
            lastMsgCall = client.BcastMsgAsync(new BcastMsgRequest { 
                Nick = this.username,
                Msg = m
            });
        }*/
    }
}
