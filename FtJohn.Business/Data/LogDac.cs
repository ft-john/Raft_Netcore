using FtJohn.Business.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business.Data
{
    public class LogDac : DataAccessComponent
    {
        public void Insert(Log log)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO Logs (Hash, [Index], Term, PrevHash, Commited, Size, Signature, Timestamp, PublicKey, SubmitterAddress, LeaderId, Category, SenderAddress, ReceiverAddress, LegalCurrency, LegalCurrencyAmount, CryptoCurrency, CryptoCurrencyAmount, ExchangeRate, TxTimestamp, TxHash) ");
            sql.Append($"VALUES('{log.Hash}',{log.Index}, {log.Term}, '{log.PrevHash}',{Convert.ToInt32(log.Commited)}, {log.SigSize}, '{log.Signature}', {log.Timestamp}, '{log.PublicKey}', '{log.SubmitterAddress}', '{log.LeaderId}', '{log.Category}', '{Convert.ToString(log.SenderAddress)}', ");
            sql.Append($"'{Convert.ToString(log.ReceiverAddress)}', '{Convert.ToString(log.LegalCurrency)}',{log.LegalCurrencyAmount},'{Convert.ToString(log.CryptoCurrency)}',{log.CryptoCurrencyAmount},{log.ExchangeRate},{log.TxTimestamp},'{log.TxHash}');");

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(sql.ToString(), con))
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Commit(string hash)
        {
            const string SQL_STATEMENT = "UPDATE Logs SET Commited = 1 WHERE Hash = @Hash";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public Log SelectById(long id)
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE Id = @Id";
            Log item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");
                    }
                }
            }

            return item;
        }

        public Log SelectByHash(string hash)
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE Hash = @Hash";
            Log item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Hash", hash);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");
                    }
                }
            }

            return item;
        }

        public Log SelectCommitedByIndex(long index)
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE [Index] = @Index AND Commited = 1";
            Log item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Index", index);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");
                    }
                }
            }

            return item;
        }
        public List<Log> SelectByIndex(long index)
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE [Index] = @Index";
            var items = new List<Log>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Index", index);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");

                        items.Add(item);
                    }
                }
            }

            return items;
        }

        public Log SelectLastCommitedLog()
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE Commited = 1 ORDER BY [Index] DESC LIMIT 1";
            Log item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");
                    }
                }
            }

            return item;
        }
        public Log SelctLastCommitedLog()
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE Commited = 1 ORDER BY [Index] DESC LIMIT 1";
            Log item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");
                    }
                }
            }

            return item;
        }

        public List<Log> SelectLogsByTerm(long term)
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE Term = @Term AND Commited = 1";
            List<Log> items = new List<Log>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Term", term);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Log item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");

                        items.Add(item);
                    }
                }
            }

            return items;
        }

        public List<Log> SelectSinceLogs(long id, int count)
        {
            const string SQL_STATEMENT = "SELECT * FROM Logs WHERE Id >= @Id LIMIT @Count";
            List<Log> items = new List<Log>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Count", count);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Log item = new Log();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Hash = GetDataValue<string>(dr, "Hash");
                        item.Index = GetDataValue<long>(dr, "Index");
                        item.Term = GetDataValue<long>(dr, "Term");
                        item.PrevHash = GetDataValue<string>(dr, "PrevHash");
                        item.Commited = GetDataValue<bool>(dr, "Commited");
                        item.SigSize = GetDataValue<int>(dr, "Size");
                        item.Signature = GetDataValue<string>(dr, "Signature");
                        item.Timestamp = GetDataValue<long>(dr, "Timestamp");
                        item.PublicKey = GetDataValue<string>(dr, "PublicKey");
                        item.SubmitterAddress = GetDataValue<string>(dr, "SubmitterAddress");
                        item.LeaderId = GetDataValue<string>(dr, "LeaderId");
                        item.Category = GetDataValue<string>(dr, "Category");
                        item.SenderAddress = GetDataValue<string>(dr, "SenderAddress");
                        item.ReceiverAddress = GetDataValue<string>(dr, "ReceiverAddress");
                        item.LegalCurrency = GetDataValue<string>(dr, "LegalCurrency");
                        item.LegalCurrencyAmount = GetDataValue<double>(dr, "LegalCurrencyAmount");
                        item.CryptoCurrency = GetDataValue<string>(dr, "CryptoCurrency");
                        item.CryptoCurrencyAmount = GetDataValue<double>(dr, "CryptoCurrencyAmount");
                        item.ExchangeRate = GetDataValue<double>(dr, "ExchangeRate");
                        item.TxTimestamp = GetDataValue<long>(dr, "TxTimestamp");
                        item.TxHash = GetDataValue<string>(dr, "TxHash");

                        items.Add(item);
                    }
                }
            }

            return items;
        }

    }
}
