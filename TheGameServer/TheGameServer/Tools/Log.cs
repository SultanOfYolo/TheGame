using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace TheGameServer
{
    class Log
    {
        private static TextWriter _textWriter;
        private static volatile object _oLock;

        public static void Initialize()
        {
            _textWriter = Console.Out;
            _oLock = new object();
        }

        public static void Write(string format, params object[] pParams)
        {
            var final = string.Format("[{0}] - {1} - ", DateTime.Now, new StackTrace().GetFrame(1).GetMethod().Name);
            lock (_oLock)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                _textWriter.Write(final);
                Console.ForegroundColor = ConsoleColor.Gray;
                _textWriter.WriteLine(format, pParams);
                _textWriter.Flush();
            }
        }
    }
}
