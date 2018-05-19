using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Threading;
using System.Net;

namespace TheGameServer
{
    public class Clients
    {
        private volatile object _lock;
        private List <Clientplayer> ClientList;

        public Clients()
        {
            _lock = new object();
            ClientList = new List<Clientplayer>();
        }

        public void AddToList(Clientplayer x)
        {
            lock (_lock)
                ClientList.Add(x);
        }

        public int Count
        {
            get
            {
                lock(_lock)
                    return ClientList.Count;
            }
        }

        public void Remove(Clientplayer x)
        {
            lock (_lock)
                ClientList.Remove(x);
        }

        public void Clear()
        {
            lock (_lock)
                ClientList .Clear();
        }

        public List<Clientplayer> GetList()
        {
            lock (_lock)
                return ClientList.GetRange(0, ClientList.Count);
        }
    }

    public class Clientplayer
    {
        private TcpClient ClientSocket;
        private NetworkStream networkStream;
        private byte[] buff;
        private List<byte> streambytes;
        private int AID;
        private Warning clientwarns;
        private short waiter;
        private string key;
        private byte[] codebyte0;// for encryption
        private byte[] codebyte; // for encryption
        private bool iscmd;
        private bool staff;

        public Clientplayer(TcpClient ClientSocket)
        {
            staff = false;
            ClientSocket.ReceiveTimeout = 100;
            this.ClientSocket = ClientSocket;
            networkStream = ClientSocket.GetStream();
            buff = new byte[ClientSocket.ReceiveBufferSize]; //8192
            //streambytes = new ArrayList();
            streambytes = new List<byte>();
            waiter = 0;
            key = String.Empty;
            AID = GetRemoteEndPointInt(this);
            if ((clientwarns = Globals.Shield.Reference(AID)) == null)
                clientwarns = new Warning(AID);
            AID = 0;
            iscmd = false;
            codebyte = new byte[16];
            codebyte0 = new byte[5];
            new Random().NextBytes(codebyte);
            new Random().NextBytes(codebyte0);
            Globals.Clients.AddToList(this);
            //IpInt = ClientHandler.GetRemoteEndPointInt(this);
        }

        public void Process()
        {
            //need to define packetprocessor before buff fills our list, need to process step packets first!
            try
            {
                if (IsVaildProcess(this))
                {
                    if (networkStream.DataAvailable)
                    {
                        if (iscmd)
                        {
                            if (Functions.IsAllowedPacketID(streambytes[0], staff)) // || cmdid.Equals(byte.MinValue))
                            {
                                Globals.Shield.Block(GetRemoteEndPointInt(this), clientwarns);
                                Disconnect(this, "Illegal PacketID!");
                                return;
                            }
                        }
                        int BytesRead = networkStream.Read(buff, 0, (int)buff.Length);

                        if (iscmd && streambytes.Count + BytesRead > GetPacketMax(streambytes[0]))
                        {
                            Globals.Shield.Block(GetRemoteEndPointInt(this), clientwarns);
                            Disconnect(this, "Illegal Packet Length");
                        }
                        else
                            streambytes.AddRange(Functions.TrimBuffer(buff, BytesRead));
                        iscmd = true;
                    }
                    else if (streambytes.Count > 0) // since our buffer is set to the default val, we may need to process the shit
                    {
                        iscmd = false;
                        ProcessPacket();
                    }
                    else if (++waiter >= 3000)
                    {
                        Globals.Shield.Block(GetRemoteEndPointInt(this), clientwarns);
                        Disconnect(this, "Client Dead");
                    }
                }
            }
            catch (Exception e)
            {
                Disconnect(this, "Client Connection Error: "+e.Message);
            }
        }

        private void ProcessPacket()
        {
            //test..
            Log.Write(Functions.GetStringOfBytes(streambytes.ToArray()));
            streambytes.Clear();
            return;
            //end test..

            waiter = 0;
            switch ((Enums.CPacketID)streambytes[0])
            {
                case Enums.CPacketID.Login:
                    string data = Functions.GetStringOfBytes(streambytes.ToArray());
                    //validate data stuct --> split data to userpass
                    string[] userpass = new string[] { "" };

                    if ((AID = Database.Login("Admin", "Adminpass", GetRemoteEndPointString(this))) < 1)
                        throw new NotImplementedException();
                    //get reses, objects, and send main map co as bool, send if needed
                    break;
                case Enums.CPacketID.Logout:
                    Disconnect(this, "Client Logged Out");
                    Database.Logout(AID);
                    return;
                case Enums.CPacketID.SelectChar:
                    throw new NotImplementedException();
                case Enums.CPacketID.GetClientPeerInfos:
                    throw new NotImplementedException();
                case Enums.CPacketID.CharSelected:
                    throw new NotImplementedException();
                case Enums.CPacketID.UpdateMapOnCo:
                    throw new NotImplementedException();
                case Enums.CPacketID .RestartServer:
                    Globals.Server.Stop(); // Server Auto Restart sys.
                    return;
                case Enums.CPacketID.FixLag :
                    Functions.FixLag();
                    return;
                default:
                    throw new NotImplementedException();
                //return;
            }


            streambytes.Clear();
        }

        public static int GetPacketMax(byte cmd) // later for cmd max length determining..
        {
            /*switch ((Enums.CPacketID)cmd)
            {
                case  Enums.CPacketID.CharSelected:
                    return 14;
            }*/

            return 8192;
        }

        public static byte[] GetCode16(Clientplayer x)
        {
            return x.codebyte;
        }

        public static byte[] GetCodeByte(Clientplayer x)
        {
            return x.codebyte0;
        }

        public static void CodeEvent(Clientplayer x)
        {
            Random efiller = new Random();
            Array.Clear(x.codebyte, 0, x.codebyte.Length);
            Array.Clear(x.codebyte0, 0, x.codebyte0.Length);
            efiller.NextBytes(x.codebyte0);
            efiller.NextBytes(x.codebyte);
        }

        public static bool IsVaildProcess(Clientplayer x)
        {
            if (Warning.GetLevel(x.clientwarns) > 10)
            {
                if (DateTime.Compare(Warning.GetTime(x.clientwarns).AddSeconds(2), DateTime.Now) > 0)
                {
                    Globals.Shield.Block(GetRemoteEndPointInt(x), x.clientwarns);
                    Disconnect(x, "Illegal Packet delays");
                    return false;
                }
                else
                    Warning.Reset(x.clientwarns); // prevent overrun
            }
            else if (Warning.GetKeyErrors(x.clientwarns) > 2)
            {
                Globals.Shield.Block(GetRemoteEndPointInt(x), x.clientwarns);
                Disconnect(x, "Client Key Error!");
                return false;
            }
            return IsAlive(x);
        }

        public static EndPoint PeerInfo(Clientplayer x)
        {
            return IsAlive(x) ? x.ClientSocket.Client.RemoteEndPoint : null;
        }

        public static string Key(Clientplayer x)
        {
            lock(x.key) // Watchers access
                return x.key;
        }

        public static bool IsAlive(Clientplayer x)
        {
            return x.networkStream != null && x.ClientSocket != null && x.ClientSocket.Client != null &&
                   x.networkStream.CanRead && x.ClientSocket.Client.IsBound;//&& ClientSocket.Client.Available > 0;
        }

        public static Int32 GetAid(Clientplayer x)
        {
            return IsAlive(x) ? x.AID : 0;
        }

        public static Warning GetWarning(Clientplayer x)
        {
            return IsAlive(x) ? x.clientwarns : null;
        }

        public static NetworkStream Stream(Clientplayer x)
        {
            return IsAlive(x) ? x.networkStream : null;
        }

        public static TcpClient Client(Clientplayer x)
        {
            return IsAlive(x) ? x.ClientSocket : null;
        }

        public static void Send(Clientplayer x, byte[] buffer, byte cmd)
        {
            if(IsAlive (x))
            {
                byte[] sendingbyter;
                if (buffer != null)
                {
                    sendingbyter = new byte[buffer.Length + 1];
                    sendingbyter[0] = cmd;
                    Buffer.BlockCopy(buffer, 0, sendingbyter, 1, buffer.Length);
                }else
                    sendingbyter = new byte[] {cmd};
                try
                {
                    x.ClientSocket.Client.Send(sendingbyter);
                }
                catch
                {
                    Disconnect(x, "Packet Sending Failed!");
                }
            }
        }

        public static void Disconnect(Clientplayer x, string msg)
        {
            if (IsAlive(x))
            {
                Log.Write("Client {0} disconnected, Reason: {1}", GetRemoteEndPointString(x), msg);
                x.networkStream.Close();
                x.ClientSocket.Close();
            }
            Globals.Clients.Remove(x);
            if (Warning.GetLevel(x.clientwarns) < 5 && Warning.GetKeyErrors(x.clientwarns) < 3)
                Globals.Shield.UnBlock(GetRemoteEndPointInt(x));
                //Warning.Terminate(x.clientwarns); // done by Garbage collector since no pointer on element if client is off
            x.streambytes.Clear();
        }

        public static string GetRemoteEndPointIntPort(Clientplayer x)
        {
            string[] Ipport = GetRemoteEndPointString(x).Split(new char[] { ':' });
            return Functions.ipToInt(Ipport[0]) + "." + Ipport[1];
        }

        public static int GetRemoteEndPointInt(Clientplayer x)
        {
            return Functions.ipToInt(GetRemoteEndPointString(x).Split(new char[] { ':' })[0]);
        }

        public static Int32 GetRemotePort(Clientplayer x)
        {
            int port;
            Int32.TryParse(GetRemoteEndPointString(x).Split(new char[] { ':' })[1], out port);
            return port;
        }

        public static int GetRemoteEndPointInt(TcpClient x)
        {
            try
            {
                return Functions.ipToInt(x.Client.RemoteEndPoint.ToString().Split(new char[] { ':' })[0]);
            }
            catch
            {
                return 0;
            }
        }

        public static string GetRemoteEndPointString(Clientplayer x)
        {
            return IsAlive(x) ? x.ClientSocket.Client.RemoteEndPoint.ToString() : "IP.IP.IP.IP:PORT";
        }

        public static void SendServerFull(Clientplayer x)
        {
            Send(x, null, (byte)Enums.PacketID.ServerFULL);
            Disconnect(x, "Server Full");
        }

        public static void SendServerFull(TcpClient x)
        {
            try
            {
                x.Client.Send(new byte[] { (byte)Enums.PacketID.ServerFULL });
                x.Close();
            }
            catch
            {
            }
        }

        public static void Broadcast(byte[] s, byte cmd)
        {
            foreach (Clientplayer temp in Globals.Clients.GetList())
                Send(temp, s, cmd);
        }

        public static void StopALL()
        {
            Broadcast(null, (byte)Enums.PacketID.shuttindown);
            Log.Write("Broadcasting Shutdown command completed");
            lock (Globals.ConnectionPool)
                while (Globals.ConnectionPool.Count > 0)
                    Clientplayer.Disconnect(Globals.ConnectionPool.Dequeue(), "Server Shutting Down");
            Globals.Clients.Clear();
            Log.Write("All Clients Disconnected!, Command Executed!");
        }

        public static string GetRemoteEndPointString(TcpClient x)
        {
            try
            {
                return x.Client.RemoteEndPoint.ToString();
            }
            catch
            {
                return "IP.IP.IP.IP:PORT";
            }
        }
    }
}
