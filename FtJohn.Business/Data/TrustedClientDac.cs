using FtJohn.Raft.Peers;
using FtJohn.Business.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business.Data
{
    public class TrustedClientDac : DataAccessComponent
    {
        public void Insert(TrustedClient client)
        {
            const string SQL_STATEMENT =
                "INSERT INTO TrustedClients (Address, Description) " +
                "VALUES(@Address, @Description)";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Address", client.Address);

                if(client.Description == null)
                {
                    cmd.Parameters.AddWithValue("@Description", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Description", client.Description);
                }

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public TrustedClient SelectByAddress(string address)
        {
            const string SQL_STATEMENT = "SELECT * FROM TrustedClients WHERE Address = @Address LIMIT 1";
            TrustedClient item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Address", address);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new TrustedClient();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Address = GetDataValue<string>(dr, "Address");
                        item.Description = GetDataValue<string>(dr, "Description");
                    }
                }
            }

            return item;
        }

        public List<TrustedClient> SelectAll()
        {
            const string SQL_STATEMENT = "SELECT * FROM TrustedClients";
            var items = new List<TrustedClient>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var item = new TrustedClient();

                        item.Id = GetDataValue<long>(dr, "Id");
                        item.Address = GetDataValue<string>(dr, "Address");
                        item.Description = GetDataValue<string>(dr, "Description");

                        items.Add(item);
                    }
                }
            }

            return items;
        }
    }
}
