using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Messages
{
    public class VoteResponse : IMessage
    {
        public long Term { get; set; }
        public bool VoteGranted { get; set; }

        public void Deserialize(byte[] bytes)
        {
            var termBytes = new byte[8];
            byte voteGrantedByte = 0x00;

            int index = 0;
            Array.Copy(bytes, index, termBytes, 0, termBytes.Length);

            index += termBytes.Length;
            voteGrantedByte = bytes[index];

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
            }

            this.Term = BitConverter.ToInt32(termBytes, 0);
            this.VoteGranted = (voteGrantedByte == 0x01);
        }

        public byte[] Serialize()
        {
            var data = new List<byte>();
            var termBytes = BitConverter.GetBytes(Term);
            var voteGrantedByte = VoteGranted ? (byte)0x01 : (byte)0x00;

           if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(termBytes);
            }

            data.AddRange(termBytes);
            data.Add(voteGrantedByte);

            return data.ToArray();
        }
    }
}
