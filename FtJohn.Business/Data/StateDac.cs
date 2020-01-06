using FtJohn.Raft.Framework;
using FtJohn.Raft.State;
using FtJohn.Business.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business.Data
{
    public class StateDac : DataAccessComponent
    {
        public void SaveState(CurentState state)
        {
            const string SQL_STATEMENT =
                "REPLACE INTO CurrentState (Id, State, CurrentTerm, VotedFor, VotesCount, CommitIndex, LastLogIndex, Timestamp, ElectedTime) " +
                "VALUES(@Id, @State, @CurrentTerm, @VotedFor, @VotesCount, @CommitIndex, @LastLogIndex, @Timestamp, @ElectedTime)";

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Parameters.AddWithValue("@Id", state.Id);
                cmd.Parameters.AddWithValue("@State", (int)state.State);
                cmd.Parameters.AddWithValue("@CurrentTerm", state.CurrentTerm);
                cmd.Parameters.AddWithValue("@VotedFor", "" + state.VotedFor);
                cmd.Parameters.AddWithValue("@VotesCount", state.VotesCount);
                cmd.Parameters.AddWithValue("@CommitIndex", state.CommitIndex);
                cmd.Parameters.AddWithValue("@LastLogIndex", state.LastLogIndex);
                cmd.Parameters.AddWithValue("@Timestamp", Time.EpochTime);

                if(state.ElectedTime != null)
                {
                    cmd.Parameters.AddWithValue("@ElectedTime", state.ElectedTime.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ElectedTime", DBNull.Value);
                }

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public CurentState SelectCurrentState()
        {
            const string SQL_STATEMENT = "SELECT * FROM CurrentState LIMIT 1";
            CurentState item = null;

            using (SqliteConnection con = new SqliteConnection(base.CacheConnectionString))
            using (SqliteCommand cmd = new SqliteCommand(SQL_STATEMENT, con))
            {
                cmd.Connection.Open();
                using (SqliteDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        item = new CurentState();

                        item.Id = GetDataValue<string>(dr, "Id");
                        item.State = (EnumState)Enum.ToObject(typeof(EnumState), GetDataValue<int>(dr, "State"));
                        item.CurrentTerm = GetDataValue<long>(dr, "CurrentTerm");
                        item.VotedFor = GetDataValue<string>(dr, "VotedFor");
                        item.VotesCount = GetDataValue<int>(dr, "VotesCount");
                        item.CommitIndex = GetDataValue<long>(dr, "CommitIndex");
                        item.LastLogIndex = GetDataValue<long>(dr, "LastLogIndex");
                        item.ElectedTime = GetDataValue<long?>(dr, "ElectedTime");
                    }
                }
            }

            return item;
        }
    }
}
