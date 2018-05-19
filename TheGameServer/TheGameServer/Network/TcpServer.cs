using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace TheGameServer
{   
    public class SocketListener
    {
        private string IP;
        private int port, procesors, watch, watchclient;
        private bool Online;

        public void Stop()
        {
            this.Online = false;
        }

        public void Start(string IP, int port, int procesors, int watch, int watchclient)
        {
            this.Online = true;
            this.IP = IP;
            this.port = port;
            this.procesors = procesors;
            this.watch = watch;
            this.watchclient = watchclient;

            new Thread(new ThreadStart(TCP_Server)).Start();
        }

        private void TCP_Server()
        {
            TcpListener listener = null;

            Log.Write("Launching TCP Server and Initializing the Network....");
            try
            {
                listener = new TcpListener(IPAddress.Parse(IP), port);
                listener.Start();
                Log.Write("Network Initialized, listening for Connections on: {0}", listener.LocalEndpoint.ToString());

                Processor.Run(procesors);

                new ProcessorWatcher().Start(watch);
                new ClientWatcher().Start(watchclient);

                Log.Write("Mission Completed Generel, you can go now!");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("**================================================================================================**");

                while (this.Online)
                {
                    TcpClient NewClient = listener.AcceptTcpClient();

                    if (Globals.Shield.IsBlocked(Clientplayer.GetRemoteEndPointInt(NewClient)))
                    {
                        try
                        {
                            NewClient.Close();
                        }
                        catch { }
                        continue;
                    }

                    if (Functions.IsServerFull)
                    {
                        Log.Write("New Player Queued, Server is FULL! ----- RemoteEndPoint: {0}", Clientplayer.GetRemoteEndPointString(NewClient));
                        Globals.ServerFullClients.Enqueue(NewClient);
                    }
                    else
                    {
                        Log.Write("New Player Connected! ----- RemoteEndPoint: {0}", Clientplayer.GetRemoteEndPointString(NewClient));
                        Globals.ConnectionPool.Enqueue(new Clientplayer(NewClient));
                    }
                }
                Shutdown(listener);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString());
                Shutdown(listener);
            }
        }

        private void Shutdown(TcpListener me)
        {
            if (me != null && me.Server.IsBound)
                me.Stop();

            Clientplayer.StopALL();
            Processor.StopAll();
            Globals.Processors.Clear();
            Globals.Clients.Clear();

            for (int i = 6; i > 0; i--)
            {
                Log.Write("We crashed! close me within {0} seconds, or i'll restart automaticly!", i);
                System.Threading.Thread.Sleep(1100);
            }

            string[] x = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Split(new char[] { '/' });
            System.Diagnostics.Process.Start(x[x.Length - 1]);
            Environment.Exit(0);
        }
    }
}