using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheGameServer
{
    class Enums
    {
        public enum CPacketID : byte // Client Packet
        {
            Login = 0x1,
            Logout = 0x2,
            SelectChar = 0x3,
            CharSelected = 0x4,
            UpdateMapOnCo = 0x5,
            GetClientPeerInfos = 0x6,
            RestartServer = 0x7,
            DisconnectPlayer = 0x8,
            FixLag = 0x9

        }

        public enum PacketID : byte // Server Packet
        {
            LoginOk = CPacketID.Login,
            LogedOut = CPacketID.Logout,
            Build = 0x3,
            Banned = 0x4,
            UpdateMapOnCo = CPacketID.UpdateMapOnCo,
            ClientPeerInfo = CPacketID.GetClientPeerInfos,
            Admin = 0x7,
            GameMaster = 0x8,
            ChatBanned = 0x9,
            ServerFULL = 0x10,
            key = 0xA,
            shuttindown = 0xB
        }
    }
}
