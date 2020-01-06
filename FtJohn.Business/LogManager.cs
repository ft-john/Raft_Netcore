using FtJohn.Raft.Framework;
using FtJohn.Raft.Log;
using FtJohn.Business.Data;
using FtJohn.Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace FtJohn.Business
{
    public class LogManager : ILogManager
    {
        #region interface implement
        public List<Log> TempLogs;
        public LogManager()
        {
            TempLogs = new List<Log>();
        }
        public void Commit(ILog log)
        {
            var logDac = new LogDac();

            var item = this.TempLogs.Where(l => l.Hash == log.Hash).FirstOrDefault();

            if(item != null)
            {
                item.Commited = true;
                logDac.Insert(item);
            }
            else
            {
                log.Commited = true;
                logDac.Insert((Log)log);
            }
            this.TempLogs.Clear();
        }

        public bool ContainsLogIndex(long index)
        {
            var dac = new LogDac();
            var result = dac.SelectByIndex(index);

            return result.Count > 0;
        }

        public bool ContainsLogHash(string hash)
        {
            var dac = new LogDac();

            var result = dac.SelectByHash(hash);

            if(result == null)
            {
                if(this.TempLogs.Where(l=>l.Hash == hash).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public bool ContainsLogTerm(long term)
        {
            var dac = new LogDac();
            var result = dac.SelectLogsByTerm(term);

            return result.Count > 0;
        }

        public void DeleteLogByHash(string hash)
        {
            //TODO: DeleteLogByHash
            //throw new NotImplementedException();
        }

        public long GetLastCommitIndex()
        {
            var dac = new LogDac();
            var log = dac.SelectLastCommitedLog();

            if(log != null)
            {
                return log.Index;
            }
            else
            {
                return -1;
            }
        }

        public ILog GetLastLog()
        {
            if (this.TempLogs.Count > 0)
            {
                return this.TempLogs[this.TempLogs.Count - 1];
            }
            else
            {
                var dac = new LogDac();
                var log = dac.SelectLastCommitedLog();

                return log;
            }
        }

        public ILog GetLastCommitedLog()
        {
            var dac = new LogDac();
            var log = dac.SelectLastCommitedLog();

            return log;
        }

        public long GetLastLogIndex()
        {
            if(this.TempLogs.Count > 0)
            {
                return this.TempLogs[this.TempLogs.Count - 1].Index;
            }
            else
            {
                var dac = new LogDac();
                var log = dac.SelectLastCommitedLog();

                if (log != null)
                {
                    return log.Index;
                }
                else
                {
                    return -1;
                }
            }
        }

        public long GetLastCommitedTerm()
        {
            var dac = new LogDac();
            var log = dac.SelectLastCommitedLog();

            if (log != null)
            {
                return log.Term;
            }
            else
            {
                return -1;
            }
        }

        public ILog GetLogByHash(string hash)
        {
            var dac = new LogDac();
            var log = dac.SelectByHash(hash);

            return log;
        }

        public ILog[] GetSinceLogs(string hash, int count)
        {
            var dac = new LogDac();

            var log = dac.SelectByHash(hash);

            if(log != null)
            {
                var result = dac.SelectSinceLogs(log.Id, count);
                return result.ToArray();
            }
            else
            {
                return new Log[0];
            }
        }

        public void HandleLog(ILog log)
        {
            if(this.TempLogs.Where(l=>l.Hash == log.Hash).Count() == 0)
            {
                this.TempLogs.Add((Log)log);
            }
        }

        public ILog LogDeserialize(byte[] bytes, ref int index)
        {
            Log log = new Log();
            var indexBytes = new byte[8];
            var hashBytes = new byte[32];
            var prevHashBytes = new byte[32];
            var termBytes = new byte[8];
            byte commitedByte = 0x00;
            var sigSizeBytes = new byte[4];
            //var signatureBytes = Base16.Decode(log.Signature);
            var timestampBytes = new byte[8];
            var publicKeyBytes = new byte[44];
            var submitterAddressBytes = new byte[28];
            var leaderIdBytes = new byte[36];
            var idBytes = new byte[8];

            Array.Copy(bytes, index, indexBytes, 0, indexBytes.Length);
            index += indexBytes.Length;

            Array.Copy(bytes, index, hashBytes, 0, hashBytes.Length);
            index += hashBytes.Length;

            Array.Copy(bytes, index, prevHashBytes, 0, prevHashBytes.Length);
            index += prevHashBytes.Length;

            Array.Copy(bytes, index, termBytes, 0, termBytes.Length);
            index += termBytes.Length;

            commitedByte = bytes[index];
            index++;

            Array.Copy(bytes, index, sigSizeBytes, 0, sigSizeBytes.Length);
            index += sigSizeBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(indexBytes);
                Array.Reverse(termBytes);
                Array.Reverse(sigSizeBytes);
            }

            log.Index = BitConverter.ToInt64(indexBytes, 0);
            log.Hash = Base16.Encode(hashBytes);
            log.PrevHash = Base16.Encode(prevHashBytes);
            log.Term = BitConverter.ToInt64(termBytes, 0);
            log.Commited = commitedByte == 0x01;
            log.SigSize = BitConverter.ToInt32(sigSizeBytes, 0);

            var signatureBytes = new Byte[log.SigSize];
            Array.Copy(bytes, index, signatureBytes, 0, signatureBytes.Length);
            index += signatureBytes.Length;

            Array.Copy(bytes, index, timestampBytes, 0, timestampBytes.Length);
            index += timestampBytes.Length;

            Array.Copy(bytes, index, publicKeyBytes, 0, publicKeyBytes.Length);
            index += publicKeyBytes.Length;

            Array.Copy(bytes, index, submitterAddressBytes, 0, submitterAddressBytes.Length);
            index += submitterAddressBytes.Length;

            Array.Copy(bytes, index, leaderIdBytes, 0, leaderIdBytes.Length);
            index += leaderIdBytes.Length;

            Array.Copy(bytes, index, idBytes, 0, idBytes.Length);
            index += idBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
                Array.Reverse(idBytes);
            }


            var categorySizeBytes = new byte[4];
            Array.Copy(bytes, index, categorySizeBytes, 0, categorySizeBytes.Length);
            index += categorySizeBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(categorySizeBytes);
            }

            var categoryBytes = new byte[BitConverter.ToInt32(categorySizeBytes, 0)];
            Array.Copy(bytes, index, categoryBytes, 0, categoryBytes.Length);
            index += categoryBytes.Length;


            var senderAddressSizeBytes = new byte[4];
            Array.Copy(bytes, index, senderAddressSizeBytes, 0, senderAddressSizeBytes.Length);
            index += senderAddressSizeBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(senderAddressSizeBytes);
            }

            var senderAddressBytes = new byte[BitConverter.ToInt32(senderAddressSizeBytes, 0)];
            Array.Copy(bytes, index, senderAddressBytes, 0, senderAddressBytes.Length);
            index += senderAddressBytes.Length;


            var receiverAddressSizeBytes = new byte[4];
            Array.Copy(bytes, index, receiverAddressSizeBytes, 0, receiverAddressSizeBytes.Length);
            index += receiverAddressSizeBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(receiverAddressSizeBytes);
            }

            var receiverAddressBytes = new byte[BitConverter.ToInt32(receiverAddressSizeBytes, 0)];
            Array.Copy(bytes, index, receiverAddressBytes, 0, receiverAddressBytes.Length);
            index += receiverAddressBytes.Length;


            var legalCurrencySizeBytes = new byte[4];
            Array.Copy(bytes, index, legalCurrencySizeBytes, 0, legalCurrencySizeBytes.Length);
            index += legalCurrencySizeBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(legalCurrencySizeBytes);
            }

            var legalCurrencyBytes = new byte[BitConverter.ToInt32(legalCurrencySizeBytes, 0)];
            Array.Copy(bytes, index, legalCurrencyBytes, 0, legalCurrencyBytes.Length);
            index += legalCurrencyBytes.Length;

            var legalCurrencyAmountBytes = new byte[8];
            Array.Copy(bytes, index, legalCurrencyAmountBytes, 0, legalCurrencyAmountBytes.Length);
            index += legalCurrencyAmountBytes.Length;


            var cryptoCurrencySizeBytes = new byte[4];
            Array.Copy(bytes, index, cryptoCurrencySizeBytes, 0, cryptoCurrencySizeBytes.Length);
            index += cryptoCurrencySizeBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(cryptoCurrencySizeBytes);
            }

            var cryptoCurrencyBytes = new byte[BitConverter.ToInt32(cryptoCurrencySizeBytes, 0)];
            Array.Copy(bytes, index, cryptoCurrencyBytes, 0, cryptoCurrencyBytes.Length);
            index += cryptoCurrencyBytes.Length;

            var cryptoCurrencyAmountBytes = new byte[8];
            Array.Copy(bytes, index, cryptoCurrencyAmountBytes, 0, cryptoCurrencyAmountBytes.Length);
            index += cryptoCurrencyAmountBytes.Length;


            var exchangeRateBytes = new byte[8];
            Array.Copy(bytes, index, exchangeRateBytes, 0, exchangeRateBytes.Length);
            index += exchangeRateBytes.Length;


            var txTimestampBytes = new byte[8];
            Array.Copy(bytes, index, txTimestampBytes, 0, txTimestampBytes.Length);
            index += txTimestampBytes.Length;


            var txHashSizeBytes = new byte[4];
            Array.Copy(bytes, index, txHashSizeBytes, 0, txHashSizeBytes.Length);
            index += txHashSizeBytes.Length;

            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(txHashSizeBytes);
            }

            var txHashBytes = new byte[BitConverter.ToInt32(txHashSizeBytes, 0)];
            Array.Copy(bytes, index, txHashBytes, 0, txHashBytes.Length);
            index += txHashBytes.Length;


            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(legalCurrencyAmountBytes);
                Array.Reverse(cryptoCurrencyAmountBytes);
                Array.Reverse(exchangeRateBytes);
            }

            log.Signature = Base16.Encode(signatureBytes);
            log.Timestamp = BitConverter.ToInt64(timestampBytes, 0);
            log.PublicKey = Base16.Encode(publicKeyBytes);
            log.SubmitterAddress = Base58.Encode(submitterAddressBytes);
            log.LeaderId = Encoding.UTF8.GetString(leaderIdBytes);
            log.Id = BitConverter.ToInt64(idBytes, 0);
            log.Category = Encoding.UTF8.GetString(categoryBytes);
            log.SenderAddress = Encoding.UTF8.GetString(senderAddressBytes);
            log.ReceiverAddress = Encoding.UTF8.GetString(receiverAddressBytes);
            log.LegalCurrency = Encoding.UTF8.GetString(legalCurrencyBytes);
            log.LegalCurrencyAmount = BitConverter.ToInt64(legalCurrencyAmountBytes, 0) / 10000.0;
            log.CryptoCurrency = Encoding.UTF8.GetString(cryptoCurrencyBytes);
            log.CryptoCurrencyAmount = BitConverter.ToInt64(cryptoCurrencyAmountBytes, 0) / 100000000.0;
            log.ExchangeRate = BitConverter.ToInt64(exchangeRateBytes, 0) / 100000000.0;
            log.TxTimestamp = BitConverter.ToInt64(txTimestampBytes, 0);
            log.TxHash = Base16.Encode(txHashBytes);

            return log;
        }

        public byte[] LogSerialize(ILog entity)
        {
            var log = (Log)entity;
            var data = new List<byte>();

            var indexBytes = BitConverter.GetBytes(log.Index);
            var hashBytes = Base16.Decode(log.Hash);
            var prevHashBytes = Base16.Decode(log.PrevHash);
            var termBytes = BitConverter.GetBytes(log.Term);
            var commitedByte = log.Commited ? (byte)0x01 : (byte)0x00;
            var sigSizeBytes = BitConverter.GetBytes(log.SigSize);
            var signatureBytes = Base16.Decode(log.Signature);
            var timestampBytes = BitConverter.GetBytes(log.Timestamp);
            var publicKeyBytes = Base16.Decode(log.PublicKey);
            var submitterAddressBytes = Base58.Decode(log.SubmitterAddress);
            var leaderIdBytes = Encoding.UTF8.GetBytes(log.LeaderId);

            var idBytes = BitConverter.GetBytes(log.Id);            
            var categoryBytes = Encoding.UTF8.GetBytes(log.Category);
            var categorySizeBytes = BitConverter.GetBytes(categoryBytes.Length);            
            var senderAddressBytes = Encoding.UTF8.GetBytes(log.SenderAddress);
            var senderAddressSizeBytes = BitConverter.GetBytes(senderAddressBytes.Length);            
            var receiverAddressBytes = Encoding.UTF8.GetBytes(log.ReceiverAddress);
            var receiverAddressSizeBytes = BitConverter.GetBytes(receiverAddressBytes.Length);            
            var legalCurrencyBytes = Encoding.UTF8.GetBytes(log.LegalCurrency);
            var legalCurrencySizeBytes = BitConverter.GetBytes(legalCurrencyBytes.Length);
            var legalCurrencyAmountBytes = BitConverter.GetBytes((long)(log.LegalCurrencyAmount * 10000));            
            var cryptoCurrencyBytes = Encoding.UTF8.GetBytes(log.CryptoCurrency);
            var cryptoCurrencySizeBytes = BitConverter.GetBytes(cryptoCurrencyBytes.Length);
            var cryptoCurrencyAmountBytes = BitConverter.GetBytes((long)(log.CryptoCurrencyAmount * 100000000));
            var exchangeRateBytes = BitConverter.GetBytes((long)(log.ExchangeRate * 100000000));
            var txTimestampBytes = BitConverter.GetBytes(log.TxTimestamp);
            var txHashBytes = Base16.Decode(log.TxHash);
            var txHashSizeBytes = BitConverter.GetBytes(txHashBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(indexBytes);
                Array.Reverse(termBytes);
                Array.Reverse(sigSizeBytes);
                Array.Reverse(timestampBytes);
                Array.Reverse(idBytes);
                Array.Reverse(categorySizeBytes);
                Array.Reverse(senderAddressSizeBytes);
                Array.Reverse(receiverAddressSizeBytes);
                Array.Reverse(legalCurrencySizeBytes);
                Array.Reverse(legalCurrencyAmountBytes);
                Array.Reverse(cryptoCurrencySizeBytes);
                Array.Reverse(cryptoCurrencyAmountBytes);
                Array.Reverse(exchangeRateBytes);
                Array.Reverse(txTimestampBytes);
                Array.Reverse(txHashSizeBytes);
            }

            data.AddRange(indexBytes);
            data.AddRange(hashBytes);
            data.AddRange(prevHashBytes);
            data.AddRange(termBytes);
            data.Add(commitedByte);
            data.AddRange(sigSizeBytes);
            data.AddRange(signatureBytes);
            data.AddRange(timestampBytes);
            data.AddRange(publicKeyBytes);
            data.AddRange(submitterAddressBytes);
            data.AddRange(leaderIdBytes);
            data.AddRange(idBytes);
            data.AddRange(categorySizeBytes);
            data.AddRange(categoryBytes);
            data.AddRange(senderAddressSizeBytes);
            data.AddRange(senderAddressBytes);
            data.AddRange(receiverAddressSizeBytes);
            data.AddRange(receiverAddressBytes);
            data.AddRange(legalCurrencySizeBytes);
            data.AddRange(legalCurrencyBytes);
            data.AddRange(legalCurrencyAmountBytes);
            data.AddRange(cryptoCurrencySizeBytes);
            data.AddRange(cryptoCurrencyBytes);
            data.AddRange(cryptoCurrencyAmountBytes);
            data.AddRange(exchangeRateBytes);
            data.AddRange(txTimestampBytes);
            data.AddRange(txHashSizeBytes);
            data.AddRange(txHashBytes);

            return data.ToArray();
        }

        public string GetLogHash(ILog entry)
        {
            var log = (Log)entry;
            var data = new List<byte>();
            var prevHashBytes = Base16.Decode(log.PrevHash);
            var categorySizeBytes = BitConverter.GetBytes(log.Category.Length);
            var categoryBytes = Encoding.UTF8.GetBytes(log.Category);
            var senderAddressSizeBytes = BitConverter.GetBytes(log.SenderAddress.Length);
            var senderAddressBytes = Encoding.UTF8.GetBytes(log.SenderAddress);
            var receiverAddressSizeBytes = BitConverter.GetBytes(log.ReceiverAddress.Length);
            var receiverAddressBytes = Encoding.UTF8.GetBytes(log.ReceiverAddress);
            var legalCurrencySizeBytes = BitConverter.GetBytes(log.LegalCurrency.Length);
            var legalCurrencyBytes = Encoding.UTF8.GetBytes(log.LegalCurrency);
            var legalCurrencyAmountBytes = BitConverter.GetBytes((long)(log.LegalCurrencyAmount * 10000));
            var cryptoCurrencySizeBytes = BitConverter.GetBytes(log.CryptoCurrency.Length);
            var cryptoCurrencyBytes = Encoding.UTF8.GetBytes(log.CryptoCurrency);
            var cryptoCurrencyAmountBytes = BitConverter.GetBytes((long)(log.CryptoCurrencyAmount * 100000000));
            var exchangeRateBytes = BitConverter.GetBytes((long)(log.ExchangeRate * 100000000));
            var txTimestampBytes = BitConverter.GetBytes(log.TxTimestamp);
            var txHashBytes = Base16.Decode(log.TxHash);
            var txHashSizeBytes = BitConverter.GetBytes(txHashBytes.Length);

            if (BitConverter.IsLittleEndian)
            {
                data.AddRange(categorySizeBytes);
                data.AddRange(categoryBytes);
                data.AddRange(senderAddressSizeBytes);
                data.AddRange(senderAddressBytes);
                data.AddRange(receiverAddressSizeBytes);
                data.AddRange(receiverAddressBytes);
                data.AddRange(legalCurrencySizeBytes);
                data.AddRange(legalCurrencyBytes);
                data.AddRange(legalCurrencyAmountBytes);
                data.AddRange(cryptoCurrencySizeBytes);
                data.AddRange(cryptoCurrencyBytes);
                data.AddRange(cryptoCurrencyAmountBytes);
                data.AddRange(exchangeRateBytes);
                data.AddRange(txTimestampBytes);
                data.AddRange(txHashSizeBytes);
                data.AddRange(txHashBytes);
            }

            data.AddRange(prevHashBytes);
            data.AddRange(categorySizeBytes);
            data.AddRange(categoryBytes);
            data.AddRange(senderAddressSizeBytes);
            data.AddRange(senderAddressBytes);
            data.AddRange(receiverAddressSizeBytes);
            data.AddRange(receiverAddressBytes);
            data.AddRange(legalCurrencySizeBytes);
            data.AddRange(legalCurrencyBytes);
            data.AddRange(legalCurrencyAmountBytes);
            data.AddRange(cryptoCurrencySizeBytes);
            data.AddRange(cryptoCurrencyBytes);
            data.AddRange(cryptoCurrencyAmountBytes);
            data.AddRange(exchangeRateBytes);
            data.AddRange(txTimestampBytes);
            data.AddRange(txHashSizeBytes);
            data.AddRange(txHashBytes);

            return Base16.Encode(HashHelper.DoubleHash(data.ToArray()));
        }

        public ILog GetLogByIndex(long index)
        {
            var dac = new LogDac();
            return dac.SelectCommitedByIndex(index);
        }
        #endregion

        public List<Log> GetLogsByTerm(long term)
        {
            var dac = new LogDac();
            return dac.SelectLogsByTerm(term);
        }

    }
}
