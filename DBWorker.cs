using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Kontur.GameStats.Server.Datatypes;

namespace Kontur.GameStats.Server
{
	public class DbWorker : IDbWorker
	{
	    private readonly SQLiteConnection sqlConnection;
	    private readonly SQLiteCommand sqlCommand;
	    private const string DbName = "GameStats.sqlite3";

		public DbWorker ()
		{
            if (!System.IO.File.Exists(DbName))
                SQLiteConnection.CreateFile(DbName);

            sqlConnection = new SQLiteConnection("Data Source= " + DbName + ";Version=3;");
            sqlConnection.Open();

            sqlCommand = new SQLiteCommand(sqlConnection);

		    this.Init();
        }

        public void Init()
	    {
	        sqlCommand.CommandText =
	            @"CREATE TABLE IF NOT EXISTS servers (endpoint TEXT PRIMARY KEY, name TEXT, gamemodes TEXT)";
	        sqlCommand.ExecuteNonQuery();
	    }

        public EndpointInfo[] GetServersInfo()
        {
            List<EndpointInfo> servers = new List<EndpointInfo>();

            sqlCommand.CommandText = "SELECT * FROM servers";
            SQLiteDataReader reader = sqlCommand.ExecuteReader();
            while(reader.Read())
            {
                string endpoint = (string)reader["endpoint"];
                string name = (string)reader["name"];
                string gamemodes = (string)reader["gamemodes"];

                servers.Add(new EndpointInfo(endpoint, new EndpointInfo.ServerInfo(name, gamemodes.Split(','))));
            }

            return servers.ToArray();
        }

        public EndpointInfo.ServerInfo GetServerInfo(string endpoint)
        {
            EndpointInfo.ServerInfo serverInfo = null;

            sqlCommand.CommandText = String.Format("SELECT name, gamemodes FROM servers WHERE endpoint = {0}", endpoint);
            SQLiteDataReader reader = sqlCommand.ExecuteReader();
            while(reader.Read())
            {
                serverInfo = new EndpointInfo.ServerInfo(reader.GetString(0), reader.GetString(1).Split(','));
                break;
            }

            return serverInfo;
        }

        public bool PutServerInfo(EndpointInfo server)
        {
            bool result = false;

            sqlCommand.CommandText = String.Format(@"INSERT OR REPLACE INTO servers VALUES (""{0}"", ""{1}"", ""{2}"")",
                server.endpoint,
                server.info.name,
                server.info.GetGameModesString()
                );

            int rows = -1;
            rows = sqlCommand.ExecuteNonQuery();
            if(rows >= 0)
            {
                result = true;
            }

            return result;
        }

        public MatchInfo GetServerMatch(string endpoint, string timestamp)
        {
            throw new NotImplementedException();
        }

        public bool PutServerMatch(MatchInfo match)
        {
            throw new NotImplementedException();
        }

        public bool PutServerMatch(string endpoint, DateTime timestamp, MatchInfo match)
        {
            throw new NotImplementedException();
        }
    }
}

