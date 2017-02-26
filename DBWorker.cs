using System.Data.SQLite;

namespace Kontur.GameStats.Server
{
	public class DbWorker
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
	}
}

