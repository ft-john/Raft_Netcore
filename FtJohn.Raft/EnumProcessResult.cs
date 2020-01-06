using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft
{
    public enum EnumProcessResult
    {
        NotFound = -1,
        Wait = 0,
        Replicating = 1,
        Commited = 2,
        Failed = 3
    }
}
