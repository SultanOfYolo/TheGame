using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net.Sockets;

namespace TheGameServer
{
    class ClientConnectionPool
    {
        private Queue SyncdQ = Queue.Synchronized(new Queue());

        public void Enqueue(Clientplayer client)
        {
            SyncdQ.Enqueue(client);
        }

        public void Enqueue(TcpClient client)
        {
            SyncdQ.Enqueue(client);
        }

        public TcpClient DequeueT()
        {
            return (TcpClient)(SyncdQ.Dequeue());
        }

        public Clientplayer Dequeue()
        {
            return (Clientplayer)(SyncdQ.Dequeue());
        }

        public int Count
        {
            get { return SyncdQ.Count; }
        }

        public object SyncRoot
        {
            get { return SyncdQ.SyncRoot; }
        }

    }
}
