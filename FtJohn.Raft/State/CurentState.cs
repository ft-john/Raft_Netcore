using FtJohn.Raft.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.State
{
    public class CurentState
    {
        public string Id { get; set; }
        //public string Address { get; set; }
        public EnumState State { get; set; }
        public long CurrentTerm { get; set; }
        public string VotedFor { get; set; }
        public int VotesCount { get; set; }
        //public List<ILog> Logs { get; set; }
        public long CommitIndex { get; set; }
        public long LastLogIndex { get; set; }
        //public int LastApplied { get; set; }
        //public int[] NextIndex { get; set; }
        //public int[] MatchIndex { get; set; }

        public long? ElectedTime { get; set; }
    }
}
