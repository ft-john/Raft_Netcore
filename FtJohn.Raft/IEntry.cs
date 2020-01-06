using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft
{
    public interface IEntry
    {
        string Hash { get; set; }
        byte[] Serialize();
        void Deserialize(byte[] data);
    }
}
