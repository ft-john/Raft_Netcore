using FtJohn.Raft.Peers;
using FtJohn.Business.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business.Data
{
    public class PeerDac : DataAccessComponent
    {
        public void Insert(PeerInfo peer)
        {
            const string SQL_STATEMENT =
                "INSERT INTO Peers (Id, IP, Port, ApiUri, IsLocal) " +
                "VALUES(@Id, @IP, @Port, @ApiUri, @IsLocal)";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", peer.Id);
                cmd.Parameters.AddWithValue("@IP", peer.IP);
                cmd.Parameters.AddWithValue("@Port", peer.Port);
                cmd.Parameters.AddWithValue("@ApiUri", peer.ApiUri);
                cmd.Parameters.AddWithValue("@IsLocal", peer.IsLocal);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public PeerInfo SelectById(string id)
        {
            const string SQL_STATEMENT = "SELECT * FROM Peers WHERE Id = @Id";
            PeerInfo item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new PeerInfo();

                        item.Id = GetDataValue<string>(dr, "Id");
                        item.IP = GetDataValue<string>(dr, "IP");
                        item.Port = GetDataValue<int>(dr, "Port");
                        item.ApiUri = GetDataValue<string>(dr, "ApiUri");
                        item.IsLocal = GetDataValue<bool>(dr, "IsLocal");
                    }
                }
            }

            return item;
        }

        public PeerInfo SelectLocalPeer()
        {
            const string SQL_STATEMENT = "SELECT * FROM Peers WHERE IsLocal = @IsLocal";
            PeerInfo item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@IsLocal", 1);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new PeerInfo();

                        item.Id = GetDataValue<string>(dr, "Id");
                        item.IP = GetDataValue<string>(dr, "IP");
                        item.Port = GetDataValue<int>(dr, "Port");
                        item.ApiUri = GetDataValue<string>(dr, "ApiUri");
                        item.IsLocal = GetDataValue<bool>(dr, "IsLocal");
                    }
                }
            }

            return item;
        }

        public PeerInfo SelectByIP(string ip)
        {
            const string SQL_STATEMENT = "SELECT * FROM Peers WHERE IsLocal = @IsLocal LIMIT 1";
            PeerInfo item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@IP", ip);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new PeerInfo();

                        item.Id = GetDataValue<string>(dr, "Id");
                        item.IP = GetDataValue<string>(dr, "IP");
                        item.Port = GetDataValue<int>(dr, "Port");
                        item.ApiUri = GetDataValue<string>(dr, "ApiUri");
                        item.IsLocal = GetDataValue<bool>(dr, "IsLocal");
                    }
                }
            }

            return item;
        }

        public List<PeerInfo> SelectAll()
        {
            const string SQL_STATEMENT = "SELECT * FROM Peers";
            var items = new List<PeerInfo>();

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@IsLocal", 1);

                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var item = new PeerInfo();

                        item.Id = GetDataValue<string>(dr, "Id");
                        item.IP = GetDataValue<string>(dr, "IP");
                        item.Port = GetDataValue<int>(dr, "Port");
                        item.ApiUri = GetDataValue<string>(dr, "ApiUri");
                        item.IsLocal = GetDataValue<bool>(dr, "IsLocal");

                        items.Add(item);
                    }
                }
            }

            return items;
        }
    }
}
