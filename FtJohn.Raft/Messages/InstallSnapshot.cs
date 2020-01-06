using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Messages
{
    public class InstallSnapshot : IMessage
    {
        public long Term { get; set; }
        public string LeaderId { get; set; }
        public long LastIncludedIndex { get; set; }
        public long LastIncludedTerm { get; set; }
        public int Offset { get; set; }
        public byte[] Data { get; set; }
        public bool Done { get; set; }

        public void Deserialize(byte[] bytes)
        {
            var termBytes = new byte[8];
            var leaderIdBytes = new byte[36];
            var lastIncludedIndexBytes = new byte[8];
            var lastIncludedTermBytes = new byte[8];
            var offsetBytes = new byte[4];

            var index = 0;
            Array.Copy(bytes, index, termBytes, 0, termBytes.Length);
            index += termBytes.Length;

            Array.Copy(bytes, index, leaderIdBytes, 0, leaderIdBytes.Length);
            index += leaderIdBytes.Length;

            Array.Copy(bytes, index, lastIncludedIndexBytes, 0, lastIncludedIndexBytes.Length);
            index += lastIncludedIndexBytes.Length;

            Array.Copy(bytes, index, lastIncludedTermBytes, 0, lastIncludedTermBytes.Length);
            index += lastIncludedTermBytes.Length;

            Array.Copy(bytes, index, offsetBytes, 0, offsetBytes.Length);
            index += offsetBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(lastIncludedIndexBytes);
                Array.Reverse(lastIncludedTermBytes);
                Array.Reverse(offsetBytes);
            }

            this.Term = BitConverter.ToInt32(termBytes, 0);
            this.LeaderId = Encoding.UTF8.GetString(leaderIdBytes);
            this.LastIncludedIndex = BitConverter.ToInt32(lastIncludedIndexBytes, 0);
            this.LastIncludedTerm = BitConverter.ToInt32(lastIncludedTermBytes, 0);
            this.Offset = BitConverter.ToInt32(offsetBytes, 0);

            this.Data = new byte[this.Offset];
            Array.Copy(bytes, index, this.Data, 0, this.Data.Length);
            index += this.Data.Length;

            this.Done = (bytes[index] == 0x01);
        }

        public byte[] Serialize()
        {
            var data = new List<byte>();
            var termBytes = BitConverter.GetBytes(Term);
            var leaderIdBytes = Encoding.UTF8.GetBytes(LeaderId);
            var lastIncludedIndexBytes = BitConverter.GetBytes(LastIncludedIndex);
            var lastIncludedTermBytes = BitConverter.GetBytes(LastIncludedTerm);
            var offsetBytes = BitConverter.GetBytes(Offset);
            

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(lastIncludedIndexBytes);
                Array.Reverse(lastIncludedTermBytes);
                Array.Reverse(offsetBytes);
            }

            data.AddRange(termBytes);
            data.AddRange(leaderIdBytes);
            data.AddRange(lastIncludedIndexBytes);
            data.AddRange(lastIncludedTermBytes);
            data.AddRange(offsetBytes);
            data.AddRange(Data);
            data.Add(Done ? (byte)0x01 : (byte)0x00);

            return data.ToArray();
        }
    }
}
