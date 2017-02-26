using System;
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
            if (System.IO.File.Exists(DbName))
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

	    public int PutServerInfo(string endpoint, string name, string gamemodes)
	    {
	        sqlCommand.CommandText =
	            $@"INSERT OR REPLACE INTO servers VALUES (""{endpoint}"", ""{name}"", ""{gamemodes}"")";

	        return sqlCommand.ExecuteNonQuery();
	    }

        public EndpointInfo[] GetServersInfo()
        {
            sqlCommand.CommandText = "SELECT * FROM server";
            SQLiteDataReader reader = sqlCommand.ExecuteReader();
            while(reader.Read())
            {

            }

            throw new NotImplementedException();
        }

        public EndpointInfo.ServerInfo GetServerInfo(string endpoint)
        {
            throw new NotImplementedException();
        }

        public bool PutServerInfo(EndpointInfo server)
        {
            throw new NotImplementedException();
        }

        public MatchInfo GetServerMatch(string endpoint, string timestamp)
        {
            throw new NotImplementedException();
        }

        public bool PutServerMatch(MatchInfo match)
        {
            throw new NotImplementedException();
        }
    }
}

