using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Peers
{
    public class CommandNames
    {
        public const string RequestVote = "RequestVote";
        public const string VoteResponse = "VoteResponse";
        public const string AppendEntries = "AppendEntries";
        public const string AppendEntriesResponse = "AppendEntriesResponse";
        public const string InstallSnapshot = "InstallSnapshot";
        public const string InstallSnapshotResponse = "InstallSnapshotResponse";
    }
}
