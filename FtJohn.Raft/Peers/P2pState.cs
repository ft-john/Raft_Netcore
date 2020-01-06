using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Peers
{
    public class P2PState
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public P2pCommand Command { get; set; }
    }
}
