using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.State
{
    public enum EnumState : int
    {
        Follower,
        Candidate,
        Leader
    }
}
