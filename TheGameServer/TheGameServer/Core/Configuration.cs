using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace TheGameServer
{
    class Configuration
    {
        public static Configuration Load()
        {
            return null;
            //return (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(File.OpenText("Config.xml"));
        }

        public DatabaseConfig Database;
        public TcpConfig Tcp;
        public UdpConfig Udp;
        public LocatorConfig Locator;
        public AgentConfig Agent;
        public ServerConfig Server;
        public ClientConfig Client;
        public PingConfig Ping;

        public class DatabaseConfig
        {
            public string Host;
            public string DatabaseName;
            public bool WindowsAuth;
            public string User;
            public string Pass;
        }

        public class TcpConfig
        {
            public string Ip;
            public UInt16 Port;
        }

        public class UdpConfig
        {
            public string Ip;
            public short Port;
        }

        public class LocatorConfig
        {
            public string Ip;
            public short Port;
        }

        public class AgentConfig
        {
            public string Ip;
            public string RemoteIp;
            public short TcpPort;
            public short UdpPort;
        }

        public class ServerConfig
        {
            public short Id;
            public short Capacity;
            public string Name;
        }

        public class ClientConfig
        {
            public int Version;
            public uint FileList;
        }

        public class PingConfig
        {
            public int Delay;
            public int Timeout;
        }
    }
}
