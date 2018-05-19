using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheGameServer;
using System.Net;

namespace TheGameServer
{
    class Program
    {
        static int Main(String[] args)
        {
            var version = "Beta v0.03";
            GC.Collect();
            Console.WindowWidth = Console.LargestWindowWidth - 20;
            Console.WindowHeight = Console.LargestWindowHeight - 5;
            Console.Title = "TheGameServer Emulator";
            Log.Initialize();
            printSplash(version);
            Globals.Initialize();
            if (Database.Initialize())
                Log.Write("Database Initialized!");

            Globals.Server = new SocketListener();
            Globals.Server.Start("127.0.0.1", 10, 100, 5000, 10000); // IP, Port, Processors Count, Watcher(Processors Delay, Client Delay)
            Log.Write("MatchServer Started, Threads are now handling..");
            return 0;
        }

        public static void printSplash(string version)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("                         _________________________");
            Console.WriteLine("                 _______/                         \\_______");
            Console.WriteLine("                /                                         \\");
            Console.WriteLine(" +-------------+                                           +-------------+");
            Console.WriteLine(" |               TheGameServer: MatchServer                              |");
            Console.WriteLine(" |                                                                       |");
            Console.WriteLine(" |               Beta Release Date: 03/03/12                             |");
            Console.WriteLine(" |               Written by: Demantor                                    |");
            Console.WriteLine(" |               Version: " + version+"                                     |");
            Console.WriteLine(" |                                                                       |");
            Console.WriteLine(" |               I love chicken :)                                       |");
            Console.WriteLine(" |               ...Server Application launching...                      |");
            Console.WriteLine(" +-------------+                                           +-------------+");
            Console.WriteLine("                \\_______                           _______/");
            Console.WriteLine("                        \\_________________________/");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("===========================");
            Console.WriteLine("        -SERVER LOG-       ");
            Console.WriteLine("===========================");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /*public static void Kill()
        {
            Globals.Server.Stop();
        }*/
    }
}
