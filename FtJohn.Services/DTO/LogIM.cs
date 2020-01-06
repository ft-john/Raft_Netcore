using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FtJohn.Services.DTO
{
    public class LogIM
    {
        public string PublicKey { get; set; }
        public string Signature { get; set; }
        
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
