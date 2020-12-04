using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GS
{
    public class ServerLogic
    {
        private const string masterPort = "10001";
        private readonly string id;
        private readonly string hostname;
        private readonly int port;
        private readonly int min_d;
        private readonly int max_d;
        private readonly string masterHostname;

        private readonly Random r = new Random();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly ManualResetEventSlim manual = new ManualResetEventSlim(true);

        private readonly Dictionary<string, string> serverList = new Dictionary<string, string>();
        private readonly Dictionary<string, string> partitionMaster = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> partitionList = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, (int vers, Dictionary<string, string> val)> data = new Dictionary<string, (int vers, Dictionary<string, string> val)>();

        private Server server;
        private GrpcChannel channel;
        private PMasterService.PMasterServiceClient pmc;

        public ServerLogic(string id, string hostname, int port, int min_d, int max_d, string masterHostname)
        {
            this.id = id;
            this.hostname = hostname;
            this.port = port;
            this.min_d = min_d;
            this.max_d = max_d;
            this.masterHostname = masterHostname;

            StartServerServer();

            RegisterInMaster();
        }

        public (string, int) Read(string objectId, string partitionId)
        {
            CheckFreeze();

            Console.WriteLine("Will read");

            Lock();
            if (!data.ContainsKey(partitionId))
                data[partitionId] = (0, new Dictionary<string, string>());

            int version = data[partitionId].vers;
            if (!data[partitionId].val.TryGetValue(objectId, out string value))
                value = "N/A";
            Unlock();

            Console.WriteLine("For partition " + partitionId + " and id " + objectId + ", i have " + value + " (version " + version + ")");
            return (value, version);
        }

        public int WriteAsMaster(string objectId, string partitionId, string value)
        {
            CheckFreeze();

            Dictionary<string, SHelperService.SHelperServiceClient> serverClients =
                new Dictionary<string, SHelperService.SHelperServiceClient>();

            int version = data.ContainsKey(partitionId) ? data[partitionId].vers + 1 : 0;

            foreach (string s_id in partitionList[partitionId])
                if (s_id != id)
                    serverClients.Add(s_id, ConnectToServer(s_id));

            Console.WriteLine("Asking for locks.");
            List<(string id, Task<LockDataReply> lockReply)> lockReplies = serverClients.Select(sk => (
                sk.Key,
                sk.Value.LockDataAsync(new LockDataRequest()).ResponseAsync
            )).ToList();

            foreach ((string id, Task<LockDataReply> resp) in lockReplies)
            {
                try
                {
                    resp.Wait();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Warning: Server {id} might me down.");
                    //if (ServerDown(id))
                    serverClients.Remove(id);
                }
            }

            Console.WriteLine("Locked. Asking to write");
            List<(string id, Task<WriteDataReply> writeReply)> writeReply = serverClients.Select(sk => (
                sk.Key,
                sk.Value.WriteDataAsync(new WriteDataRequest
                {
                    ObjectId = objectId,
                    PartitionId = partitionId,
                    NewObject = new Object { Value = value },
                    Version = version
                }).ResponseAsync
            )).ToList();

            foreach ((string id, Task<WriteDataReply> resp) in writeReply)
            {
                try
                {
                    resp.Wait();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Warning: Server {id} might me down.");
                    //if (ServerDown(id))
                    serverClients.Remove(id);
                }
            }

            Console.WriteLine("Unlocked and written in others.");

            Lock();
            Write(objectId, partitionId, value, version);
            Unlock();

            return version;
        }

        public void Write(string objectId, string partitionId, string newObj, int version)
        {
            CheckFreeze();

            Console.WriteLine("Starting to actually write...");

            Dictionary<string, string> newPart =
                data.ContainsKey(partitionId) ? data[partitionId].val : new Dictionary<string, string>();
            newPart.Add(objectId, newObj);

            data[partitionId] = (version, newPart);

            Console.WriteLine("Partition " + partitionId + " and id " + objectId + ", now I have " + newObj + " (version " + version + ")");

            Console.WriteLine("Done.");
        }

        public List<(string id, bool master)> List()
        {
            CheckFreeze();

            List<(string id, bool master)> list = new List<(string id, bool master)>();
            foreach (string p in data.Keys)
                foreach (string obj_id in data[p].val.Keys)
                    list.Add((obj_id, partitionList[p][0] == id));
            return list;
        }

        public void Crash()
        {
            CheckFreeze();

            Task.Run(() =>
            {
                server.KillAsync();
                Console.WriteLine("Dead.");
            });

            //System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            //process.CloseMainWindow();
        }

        public void Freeze()
        {
            manual.Reset();
            Console.WriteLine("Frozen.");
        }
        public void Unfreeze()
        {
            manual.Set();
            Console.WriteLine("Let it go!");
        }

        public void StartServerServer()
        {
            server = new Server
            {
                Services = {
                    GSService.BindService(new GServerService(this)),
                    SHelperService.BindService(new ServerHelperService(this)),
                    PNodeService.BindService(new PuppetNodeService(this)),
                    PServerService.BindService(new PuppetServerService(this))
                },
                Ports = { new ServerPort(hostname, port, ServerCredentials.Insecure) }
            };

            server.Start();

            Console.WriteLine("Insecure Server server listening on port " + port);

            //Configuring HTTP for client connections in Register method
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public void RegisterInMaster()
        {
            Console.WriteLine("Master, i'm ready for you!");
            Console.WriteLine("Waiting for some info on the network");

            channel = GrpcChannel.ForAddress($"http://{masterHostname}:{masterPort}");
            pmc = new PMasterService.PMasterServiceClient(channel);
            pmc.Register(new RegisterRequest { Id = id, Type = NodeType.Server });
        }

        public SHelperService.SHelperServiceClient ConnectToServer(string id)
        {
            Console.WriteLine("Will connect to server " + id);

            channel = GrpcChannel.ForAddress(serverList[id]);
            return new SHelperService.SHelperServiceClient(channel);
        }

        public bool Lock(bool verbose = false)
        {
            CheckFreeze();

            if (semaphore.CurrentCount == 0) Console.WriteLine("Full. Waiting...");
            semaphore.Wait();
            if (verbose) Console.WriteLine("Locked.");
            return true;
        }

        public bool Unlock(bool verbose = false)
        {
            CheckFreeze();

            semaphore.Release();
            if (verbose) Console.WriteLine("Unlocked");
            return true;
        }

        public void CheckFreeze()
        {
            if (!manual.IsSet)
                Console.WriteLine("This frozen. Will wait a bit...");
            manual.Wait();
        }

        public void StorePartitions(Dictionary<string, List<string>> parts)
        {
            CheckFreeze();

            Lock();
            foreach (string p_id in parts.Keys)
            {
                this.partitionList[p_id] = parts[p_id];
                this.partitionMaster.Add(p_id, parts[p_id][0]);
            }
            Unlock();
        }

        public void StoreServers(Dictionary<string, string> servers)
        {
            CheckFreeze();

            Lock();
            foreach (string s_id in servers.Keys)
                this.serverList[s_id] = servers[s_id];
            Unlock();
        }

        public void Status()
        {
            CheckFreeze();

            Console.WriteLine($"> Servers:");
            Console.WriteLine($"> {string.Join(", ", serverList.Keys)}");

            Console.WriteLine($"> Partitions:");
            foreach (string p_id in partitionList.Keys)
                Console.WriteLine($"> Partition {p_id} ({string.Join(", ", partitionMaster[p_id])}) is in {string.Join(", ", partitionList[p_id])}");

        }

        public void DelayMessage()
        {
            if (min_d != 0 && max_d != 0)
            {
                Console.WriteLine("(reading and parsing incoming message)");
                Task.Delay(r.Next(min_d, max_d)).Wait();
            }

        }
    }
}

