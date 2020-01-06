using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Peers
{
    public class CommandQueueItem
    {
        public CommandQueueItem(string ip, int port, P2pCommand cmd)
        {
            this.IP = ip;
            this.Port = port;
            this.Command = cmd;
        }

        public string IP { get; set; }
        public int Port { get; set; }
        public P2pCommand Command { get; set; }
    }
}
