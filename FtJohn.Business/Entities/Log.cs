using FtJohn.Raft.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business.Entities
{
    public class Log : ILog
    {
        public long Index { get; set; }
        public string Hash { get; set; }
        public string PrevHash { get; set; }
        public long Term { get; set; }
        public bool Commited { get; set; }
        public int SigSize { get; set; }
        public string Signature { get; set; }
        public long Timestamp { get; set; }
        public string PublicKey { get; set; }
        public string SubmitterAddress { get; set; }
        public string LeaderId { get; set; }

        public long Id { get; set; }
        public string Category { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverAddress { get; set; }
        public string LegalCurrency { get; set; }
        public double LegalCurrencyAmount { get; set; }
        public string CryptoCurrency { get; set; }
        public double CryptoCurrencyAmount { get; set; }
        public double ExchangeRate { get; set; }
        public long TxTimestamp { get; set; }
        public string TxHash { get; set; }
    }
}
