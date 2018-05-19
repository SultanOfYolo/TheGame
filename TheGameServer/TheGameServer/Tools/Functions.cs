using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TheGameServer
{
    class Functions
    {
        public static byte[] TrimBuffer(byte[] bytebuffer, int index)
        {
            if (bytebuffer == null)
                return null;
            else if (index == bytebuffer.Length)
                return bytebuffer;
            byte[] newbyte = new byte[index];
            Buffer.BlockCopy(bytebuffer, 0, newbyte, 0, index);
            return newbyte;

            /*if (bytebuffer == null)
                return null;
            else if (index == bytebuffer.Length - 1)
                return bytebuffer;
            Array.Resize<byte>(ref bytebuffer, index);
            return bytebuffer;*/
        }

        /*public static byte[] GetCopyOfTrimedByterSize(byte[] x)
        {
            int rlen = Array.IndexOf(x, byte.MinValue);
            if (rlen == x.Length-1 || rlen < 0)
                return x;
            byte[] rx = new byte[rlen];
            Array.Copy(x, rx, rlen);
            return rx;

            //int len = x.Length;
            //List<byte> list = new List<byte>(len); // len?
            //for (int i = 0; i < len; i++)
                //if (x[i] == byte.MinValue)
                    //break;
                //else
                    //list.Add(x[i]);
            //return list.ToArray();
        }*/

        public static void FixLag()
        {
            new Thread(new ThreadStart(LagFix)).Start();
        }


        private static void LagFix()
        {
            GC.Collect();
        }

        public static bool IsClientPacketID(byte ID)
        {
            return Enum.IsDefined(typeof(Enums.CPacketID), ID);
        }

        public static bool IsAllowedPacketID(byte ID, bool staff)
        {
            return IsClientPacketID(ID) && (IsStaffCmd(ID)? staff? true:false:true);
            /*if (IsClientPacketID(ID))
                if (!staff && IsStaffCmd(ID))
                    return false;
                else
                    return true;
            return false;*/
        }

        public static bool IsStaffCmd(byte x)
        {
            switch ((Enums.CPacketID)x)
            {
                case Enums.CPacketID.RestartServer:
                case Enums.CPacketID .DisconnectPlayer:
                    return true;
                default:
                    return false;
            }
        }

        public static string intToIp(int i)
        {
            return ((i >> 24) & 0xFF) + "." +

                   ((i >> 16) & 0xFF) + "." +

                   ((i >> 8) & 0xFF) + "." +

                   (i & 0xFF);
        }

        /*public static void Null(object o)
        {
            o = null;
        }*/

        public static bool IsServerFull
        {
            get
            {
                return Globals.Clients.Count >= Globals.ServerMax;
            }
        }

        public static string GetStringOfBytes(byte[] o)
        {
            return Encoding.ASCII.GetString(o);
        }

        public static int ipToInt(string addr)
        {
            string[] addrArray = addr.Split(new char[] { '.' });
            short[] ips = new short[4];
            for(int i=0; i < 4; i++)
                if (!Int16.TryParse(addrArray[i], out ips[i]))
                    return 0;
            int num = 0, power = 3;
            for (int i = 0; i < 4; i++)
            {
                num += (int)(ips[i] % 256 * Math.Pow(256, power));
                power = 3 - i;
            }
            return num;
        }
    }
}
