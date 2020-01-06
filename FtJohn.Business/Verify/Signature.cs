using FtJohn.Raft.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FtJohn.Business.Verify
{
    public class Signature
    {
        public static bool Verify(string publicKey, string signedResult, string source)
        {
            ECDsa dsa = ECDsa.ImportPublicKey(Base16.Decode(publicKey));
            return dsa.VerifyData(Encoding.UTF8.GetBytes(source), Base16.Decode(signedResult));
        }
    }
}
