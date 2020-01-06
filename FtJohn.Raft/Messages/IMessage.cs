using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Messages
{
    public interface IMessage
    {
        byte[] Serialize();

         void Deserialize(byte[] bytes);
    }
}
