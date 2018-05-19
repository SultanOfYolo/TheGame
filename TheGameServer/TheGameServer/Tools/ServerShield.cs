using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

namespace TheGameServer
{
    class ServerShield
    {
        private volatile object _lock;
        private List<Pair<Int32, Warning>> Addresses;

        public ServerShield()
        {
            _lock = new object();
            Addresses = new List<Pair<Int32, Warning>>();
        }

        public void AddClientWatch(Int32 c, Warning player)
        {
            lock(_lock)
                Addresses.Add(new Pair<Int32, Warning>(c, player));
        }

        public void Block(Int32 c, Warning player)
        {
            Pair<Int32, Warning> obj = IndexOfObject(c);
            lock (_lock)
            {
                if (obj == null)
                    Addresses.Add(new Pair<Int32, Warning>(c, player));// != null ? player : null)); // fix bug, object locked & null player
                Warning.AddWarningToClient(player);
            }
        }

        public List<Pair<int, Warning>> GetList()
        {
            lock (_lock)
                return Addresses.GetRange(0, Addresses.Count);
        }

        public bool Contains(Int32 IP)
        {
            return null != IndexOfObject(IP);// != null;
        }

        public bool Contains(Warning player)
        {
            return null != IndexOfObject(player);// != null;
        }

        private Pair<Int32, Warning> IndexOfObject(Int32 c)
        {
            lock (_lock)
                if(c>0)
                    for (int i = 0; i < Addresses.Count; i++)
                        if (Addresses[i] != null && Addresses[i].First.Equals(c))
                            return Addresses[i];
            return null;
        }

        public Warning Reference(Int32 IP)
        {
            Pair<Int32, Warning> obj = IndexOfObject(IP);
            lock (_lock)
                if (obj != null && obj.Second != null)
                    return obj.Second;
            return null;
        }

        private Pair<Int32, Warning> IndexOfObject(Warning player)
        {
            lock (_lock)
                if(player != null)
                    for (int i = 0; i < Addresses.Count; i++)
                        if (Addresses[i] != null && Addresses[i].Second != null && Addresses[i].Second.Equals(player))
                            return Addresses[i];
            return null;
        }

        public void UnBlock(Int32 c)
        {
            Pair<Int32, Warning> obj = IndexOfObject(c);
            lock (_lock)
                if (obj != null)
                    Addresses.Remove(obj);
        }

        public void UnBlock(Warning player)
        {
            Pair<Int32, Warning> obj = IndexOfObject(player);
            lock (_lock)
                if (obj != null)
                    Addresses.Remove(obj);
        }

        public bool IsBlocked(Int32 c)
        {
            Pair<Int32, Warning> obj = IndexOfObject(c);
            lock (_lock)
                if (obj != null && obj.Second != null)
                    return Warning.GetLevel(obj.Second) >= 5;
            return false;
        }

        public bool IsBlocked(Warning player)
        {
            Pair<Int32, Warning> obj = IndexOfObject(player);
            lock (_lock)
                if (obj != null)
                    return Warning.GetLevel(obj.Second) >= 5;
            return false;
        }
    }

    public class Pair<T1, T2>
    {
        public T1 First; // IP int
        public T2 Second; // Warning obj

        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }
}
