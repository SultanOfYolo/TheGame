using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace TheGameServer
{
    public class Processor
    {
        private bool process;

        public void Start()
        {
            process = true;
            Globals.Processors.Add(this);
            new Thread(new ThreadStart(RunProcessor)).Start();
        }

        public static bool IsRunning(Processor x)
        {
            return (x != null && x.process);
        }

        public static void Terminate(Processor x)
        {
            x.process = false;
        }

        private void RunProcessor()
        {
            while (process)
            {
                Clientplayer client = null;
                lock (Globals.ConnectionPool.SyncRoot)
                {
                    if (Globals.ConnectionPool.Count > 0)
                        client = Globals.ConnectionPool.Dequeue();
                }

                if (client != null)
                {
                    client.Process();
                    if (Clientplayer.IsAlive(client))
                        Globals.ConnectionPool.Enqueue(client);
                }
                //Monitor.Pulse(this);
                if (Monitor.TryEnter(Globals.ServerFullClients.SyncRoot, 10))//fix bug, holding processors from client handling by full server
                {
                    //lock (Globals.ServerFullClients.SyncRoot)
                    //{
                        while (Globals.ServerFullClients.Count > 0)
                            Clientplayer.SendServerFull(Globals.ServerFullClients.DequeueT());
                    //}
                        Monitor.Exit(Globals.ServerFullClients.SyncRoot);
                }
                Thread.Sleep(100); // save CPU XDD
            }
        }

        public static void Run(int num)
        {
            if (num == 0)
                return;
            else if (num > 0)
                for (int i = 0; i < num; i++)
                    new Processor().Start();
            else if ((num = -num) < Globals.Processors.Count)
                for (int i = 0; i < num && i < Globals.Processors.Count; i++)
                    if (Processor.IsRunning(Globals.Processors[i]))
                    {
                        Processor.Terminate(Globals.Processors[i]);
                        Globals.Processors.RemoveAt(i);
                    }
                    else
                        num++;
        }

        public static void StopAll()
        {
            Log.Write("*Server Halt Command Execution, Termination of {0} running processors", Globals.Processors.Count);

            for (int i = 0; i < Globals.Processors.Count; i++)
                if (Globals.Processors[i] != null)
                    Processor.Terminate(Globals.Processors[i]);

            Globals.Processors.Clear();
            Log.Write("Server Halt Command Executed, running processors {0}", Globals.Processors.Count);
        }
    }
}
