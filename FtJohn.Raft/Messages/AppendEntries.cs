using FtJohn.Raft.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Messages
{
    public class AppendEntries : IMessage
    {
        public long Term { get; set; }
        public string LeaderId { get; set; }
        public long PrevCommitedIndex { get; set; }
        public long PrevCommitedTerm { get; set; }
        public long LeaderCommit { get; set; }
        public int Offset { get; set; }
        public byte[] LogData { get; set; }

        public void Deserialize(byte[] bytes)
        {
            var termBytes = new byte[8];
            var leaderIdBytes = new byte[36];
            var prevLogIndexBytes = new byte[8];
            var prevLogTermBytes = new byte[8];
            var leaderCommitBytes = new byte[8];
            var offsetBytes = new byte[4];

            var index = 0;

            Array.Copy(bytes, index, termBytes, 0, termBytes.Length);
            index += termBytes.Length;

            Array.Copy(bytes, index, leaderIdBytes, 0, leaderIdBytes.Length);
            index += leaderIdBytes.Length;

            Array.Copy(bytes, index, prevLogIndexBytes, 0, prevLogIndexBytes.Length);
            index += prevLogIndexBytes.Length;

            Array.Copy(bytes, index, prevLogTermBytes, 0, prevLogTermBytes.Length);
            index += prevLogTermBytes.Length;

            Array.Copy(bytes, index, leaderCommitBytes, 0, leaderCommitBytes.Length);
            index += leaderCommitBytes.Length;

            Array.Copy(bytes, index, offsetBytes, 0, offsetBytes.Length);
            index += offsetBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(prevLogIndexBytes);
                Array.Reverse(prevLogTermBytes);
                Array.Reverse(leaderCommitBytes);
                Array.Reverse(offsetBytes);
            }

            this.Term = BitConverter.ToInt32(termBytes, 0);
            this.LeaderId = Encoding.UTF8.GetString(leaderIdBytes);
            this.PrevCommitedIndex = BitConverter.ToInt32(prevLogIndexBytes, 0);
            this.PrevCommitedTerm = BitConverter.ToInt32(prevLogTermBytes, 0);
            this.LeaderCommit = BitConverter.ToInt32(leaderCommitBytes, 0);
            this.Offset = BitConverter.ToInt32(offsetBytes, 0);

            if(this.Offset > 0)
            {
                this.LogData = new byte[Offset];
                Array.Copy(bytes, index, this.LogData, 0, this.LogData.Length);
                index += this.LogData.Length;
            }
            else
            {
                this.LogData = null;
            }

        }

        public byte[] Serialize()
        {
            var data = new List<byte>();

            var termBytes = BitConverter.GetBytes(this.Term);
            var leaderIdBytes = Encoding.UTF8.GetBytes(this.LeaderId);
            var prevLogIndexBytes = BitConverter.GetBytes(PrevCommitedIndex);
            var prevLogTermBytes = BitConverter.GetBytes(this.PrevCommitedTerm);
            var leaderCommitBytes = BitConverter.GetBytes(this.LeaderCommit);
            var offsetBytes = BitConverter.GetBytes(this.Offset);

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(prevLogIndexBytes);
                Array.Reverse(prevLogTermBytes);
                Array.Reverse(leaderCommitBytes);
                Array.Reverse(offsetBytes);
            }

            data.AddRange(termBytes);
            data.AddRange(leaderIdBytes);
            data.AddRange(prevLogIndexBytes);
            data.AddRange(prevLogTermBytes);
            data.AddRange(leaderCommitBytes);
            data.AddRange(offsetBytes);

            if(Offset > 0)
            {
                data.AddRange(LogData);
            }

            return data.ToArray();
        }
    }
}
