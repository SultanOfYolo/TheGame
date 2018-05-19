using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheGameServer
{
    public class Warning
    {
        private DateTime Date;
        private Int16 Level;
        private Int16 keyerrors;

        public Warning(int ip)
        {
            Date = DateTime.Now;
            Level = 0;
            Globals.Shield.AddClientWatch(ip, this);
        }

        public static Int16 GetKeyErrors(Warning player)
        {
            return IsReal(player) ? player.keyerrors : (short)0;
        }

        public static void AddWarningToClient(Warning player, bool keyerror = false)
        {
            if (IsReal(player))
            {
                player.Level++;
                player.Date = DateTime.Now;
                if (keyerror)
                    player.keyerrors++;
            }
        }

        public static Int16 GetLevel(Warning player)
        {
            return IsReal(player) ? player.Level : (short)0;
        }

        public static DateTime GetTime(Warning player)
        {
            return IsReal(player) ? player.Date : DateTime.MinValue;
        }

        public static void Reset(Warning player)
        {
            if (IsReal(player))
            {
                player.Level = 0;
                player.keyerrors = 0;
                player.Date = DateTime.MinValue;
            }
        }

        private static bool IsReal(Warning player)
        {
            return player != null;// && Globals.Warnings.IndexOf(player) >= 0;
        }

        public static void Terminate(Warning player)
        {
            if (IsReal(player))
            {
                Globals.Shield.UnBlock(player);
                player = null;//Globals.Warnings.Remove(player);
            }
        }
    }
}
