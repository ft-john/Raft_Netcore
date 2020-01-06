using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business.Data
{
    class DBManager
    {
        public static void Initialization()
        {
            var sql = Resource.InitScript;

            using (SqliteConnection con = new SqliteConnection(GlobalParameters.IsTestnet ?
                Resource.TestnetConnectionString : Resource.MainnetConnectionString))
            {
                con.Open();
                using (SqliteCommand cmd = new SqliteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
