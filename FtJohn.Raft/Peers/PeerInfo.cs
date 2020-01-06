using FtJohn.Raft.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Peers
{
    public class PeerInfo
    {
        public string Id { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public bool IsLocal { get; set; }
        public string ApiUri { get; set; }
    }
}
