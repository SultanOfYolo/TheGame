using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TheGameServer
{
    class Globals
    {
        public static int ServerMax;
        public static Configuration Config;
        public static SocketListener Server;
        public static List<Processor> Processors;
        public static Clients Clients;
        public static ClientConnectionPool ConnectionPool;
        public static ClientConnectionPool ServerFullClients;
        public static ServerShield Shield;

        public static void Initialize()
        {
            ServerMax = 500;//Database.GetMaxPlayers(Config.Server.Id);
            Config = Configuration.Load();
            Processors = new List<Processor>();
            Log.Write("Processors Initialize: Sucess");
            Clients = new Clients();
            Log.Write("Clients Initialized & Synchronized: Sucess");
            Shield = new ServerShield();
            Log.Write("Server Shield Initialized & Synchronized: Sucess");
            ConnectionPool = new ClientConnectionPool();
            Log.Write("Client Queue Initialized & Synchronized: Sucess");
            ServerFullClients = new ClientConnectionPool();
            Log.Write("Client ServerFullQueue Initialized & Synchronized: Sucess");
            Log.Write("Global ClientServices Initialized");
        }
    }
}
