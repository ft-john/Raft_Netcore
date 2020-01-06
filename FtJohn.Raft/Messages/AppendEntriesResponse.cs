using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Messages
{
    public class AppendEntriesResponse : IMessage
    {
        public long Term { get; set; }
        public long LastLogInex { get; set; }
        public long CommitIndex { get; set; }
        public bool Success { get; set; }

        public void Deserialize(byte[] bytes)
        {
            var termBytes = new byte[8];
            var lastLogIndexBytes = new byte[8];
            var commitIndexBytes = new byte[8];
            int index = 0;

            Array.Copy(bytes, index, termBytes, 0, termBytes.Length);
            index += termBytes.Length;

            Array.Copy(bytes, index, lastLogIndexBytes, 0, lastLogIndexBytes.Length);
            index += lastLogIndexBytes.Length;

            Array.Copy(bytes, index, commitIndexBytes, 0, commitIndexBytes.Length);
            index += commitIndexBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(lastLogIndexBytes);
                Array.Reverse(commitIndexBytes);
            }

            this.Term = BitConverter.ToInt32(termBytes, 0);
            this.LastLogInex = BitConverter.ToInt32(lastLogIndexBytes, 0);
            this.CommitIndex = BitConverter.ToInt32(commitIndexBytes, 0);
            this.Success = (bytes[index] == 0x01);
        }

        public byte[] Serialize()
        {
            var data = new List<byte>();

            var termBytes = BitConverter.GetBytes(Term);
            var lastLogIndexBytes = BitConverter.GetBytes(LastLogInex);
            var commitIndexBytes = BitConverter.GetBytes(CommitIndex);

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(lastLogIndexBytes);
                Array.Reverse(commitIndexBytes);
            }

            data.AddRange(termBytes);
            data.AddRange(lastLogIndexBytes);
            data.AddRange(commitIndexBytes);
            data.Add(Success ? (byte)0x01 : (byte)0x00);

            return data.ToArray();
        }
    }
}
