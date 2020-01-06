using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Messages
{
    public class RequestVote : IMessage
    {
        public long Term { get; set; }
        public string CandidateId { get; set; }
        public long LastCommitedIndex { get; set; }
        public long LastLogTerm { get; set; }

        public void Deserialize(byte[] bytes)
        {
            var termBytes = new byte[8];
            var candidateIdBytes = new byte[36];
            var lastCommitedIndexBytes = new byte[8];
            var lastLogTermBytes = new byte[8];

            int index = 0;
            Array.Copy(bytes, index, termBytes, 0, termBytes.Length);
            index += termBytes.Length;

            Array.Copy(bytes, index, candidateIdBytes, 0, candidateIdBytes.Length);
            index += candidateIdBytes.Length;

            Array.Copy(bytes, index, lastCommitedIndexBytes, 0, lastCommitedIndexBytes.Length);
            index += lastCommitedIndexBytes.Length;

            Array.Copy(bytes, index, lastLogTermBytes, 0, lastLogTermBytes.Length);
            index += lastLogTermBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(lastCommitedIndexBytes);
                Array.Reverse(lastLogTermBytes);
            }

            this.Term = BitConverter.ToInt32(termBytes, 0);
            this.CandidateId = Encoding.UTF8.GetString(candidateIdBytes);
            this.LastCommitedIndex = BitConverter.ToInt32(lastCommitedIndexBytes, 0);
            this.LastLogTerm = BitConverter.ToInt32(lastLogTermBytes, 0);
        }

        public byte[] Serialize()
        {
            var data = new List<byte>();

            var termBytes = BitConverter.GetBytes(this.Term);
            var candidateIdBytes = Encoding.UTF8.GetBytes(CandidateId);
            var lastCommitedIndexBytes = BitConverter.GetBytes(LastCommitedIndex);
            var lastLogTermBytes = BitConverter.GetBytes(LastLogTerm);

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
                Array.Reverse(lastCommitedIndexBytes);
                Array.Reverse(lastLogTermBytes);
            }

            data.AddRange(termBytes);
            data.AddRange(candidateIdBytes);
            data.AddRange(lastCommitedIndexBytes);
            data.AddRange(lastLogTermBytes);

            return data.ToArray();
        }
    }
}
