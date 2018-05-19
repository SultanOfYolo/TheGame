using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TheGameServer
{
    public class ProcessorWatcher
    {
        private int watch_delay;
        private int Pcount;
        private int Ccount;

        public void Start(int delay)
        {
            watch_delay = delay;
            Pcount = Globals.Processors.Count;
            new Thread(new ThreadStart(ProcessorsWatcher)).Start();
        }

        private void ProcessorsWatcher()
        {
            int cmp;
            do
            {
                cmp = (Ccount = Globals.Clients.Count) - (int)Math.Round(Pcount * 1.5);
                if (cmp < 0)
                    Processor.Run(-1);
                else if(cmp> 1)
                    Processor.Run(1 + (int)Ccount / 3);
                if (Pcount != (Pcount = Globals.Processors.Count))
                    Log.Write("Running Processors: [{0}] -- Online players: [{1}]", Pcount, Ccount);
                Thread.Sleep(watch_delay);
            } while (true);
        }
    }

    public class ClientWatcher
    {
        private int watch_delay;
        private Random Rand;

        public void Start(int delayy)
        {
            watch_delay = delayy;
            Rand = new Random();
            new Thread(new ThreadStart(ClientsWatcher)).Start();
            new Thread(new ThreadStart(ClientWarningExpireWatcher)).Start();
        }

        private string CreateRandomPassword(int passwordLength)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
            char[] chars = new char[passwordLength];

            for (int i = 0; i < passwordLength; i++)
                chars[i] = allowedChars[Rand.Next(0, allowedChars.Length)];

            return new string(chars);
        }

        private string Decrypt(string key)
        {
            // cryption class
            return key;
        }

        private string getkey(string ip)
        {
            return FileManager.readtext("ClientKeys\\" + ip + ".key");
        }

        private byte[] UpdateKey(string ip)
        {
            string key = CreateRandomPassword(Rand.Next(10, 100));
            FileManager.writetext("ClientKeys\\" + ip + ".key", key);
            return Encoding.ASCII .GetBytes (key);
        }

        private void ClientWarningExpireWatcher()
        {
            Thread.Sleep(3600000); // 1 Hour
            foreach (Pair<Int32, Warning> item in Globals.Shield.GetList())
                if(Globals.Shield.IsBlocked (item.Second))
                    if (DateTime.Compare(Warning.GetTime(item.Second).AddHours(10), DateTime.Now) > 0)
                        Globals.Shield.UnBlock(item.Second);
            Functions.FixLag();
        }

        private void ClientsWatcher() // Heartbeats
        {
            bool check = false;
            short c = 0;
            do
            {
                Thread.Sleep(watch_delay);
                if (Globals.Clients.Count > 0)
                    if (c > 99)
                    {
                        foreach (Clientplayer temp in Globals.Clients.GetList())
                            Clientplayer.CodeEvent(temp);
                        c = 0;
                    }
                    else
                    {
                        foreach (Clientplayer temp in Globals.Clients.GetList())
                            if (check)
                            {
                                if (Clientplayer.IsAlive(temp))
                                    //lock(temp)
                                    if (!Clientplayer.Key(temp).Equals(Cryption.Decrypt(
                                        getkey(Clientplayer.GetRemoteEndPointIntPort(temp)),
                                        Clientplayer.GetRemoteEndPointString(temp),
                                        Clientplayer.GetCode16(temp),
                                        "SHA1",
                                        10,
                                        Clientplayer.GetCodeByte(temp),
                                        192)))
                                        Warning.AddWarningToClient(Clientplayer.GetWarning(temp), true);
                            }
                            else
                                Clientplayer.Send(temp, UpdateKey(Clientplayer.GetRemoteEndPointIntPort(temp)), (byte)Enums.PacketID.key);
                            check = !check;
                            c++;
                        }
            } while (true);
        }
    }

    public class FileManager // for later ClientHandlers' access
    {
        private static volatile object _lock = new object();

        public static void writetext(string filename, string content)
        {
            lock (_lock)
                System.IO.File.WriteAllText(filename, content);
        }

        public static string readtext(string filename)
        {
            lock (_lock)
                if (System.IO.File.Exists(filename))
                    return System.IO.File.ReadAllText(filename);
            return String.Empty;
        }

        public static string[] readlines(string filename, string content)
        {
            lock (_lock)
                return System.IO.File.ReadAllLines(filename);
        }

        public static void appendtext(string filename, string content)
        {
            lock (_lock)
                System.IO.File.AppendAllText(filename, content);
        }

        public static void appendlines(string filename, string[] content)
        {
            lock (_lock)
                System.IO.File.AppendAllLines(filename, content);
        }
    }
}
