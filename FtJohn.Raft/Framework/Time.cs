using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Framework
{
    public static class Time
    {
        public static DateTime EpochStartTime => new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        public static long EpochTime => (long)(DateTime.UtcNow - EpochStartTime).TotalMilliseconds;

        public static long UnixTime => DateTimeOffset.Now.ToUnixTimeMilliseconds();

    }
}
