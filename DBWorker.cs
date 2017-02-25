namespace Kontur.GameStats.Server
{
	public class DBWorker
	{
	    private readonly System.Data.SQLite.SQLiteConnection sqlConnection;
	    private readonly System.Data.SQLite.SQLiteCommand sqlCommand;
	    private const string dbName = "GameStats.sqlite";

		public DBWorker ()
		{
            if (System.IO.File.Exists(dbName))
                System.Data.SQLite.SQLiteConnection.CreateFile(dbName);

            System.Data.SQLite.SQLiteFactory factory = (System.Data.SQLite.SQLiteFactory) System.Data.Common.DbProviderFactories.GetFactory("System.Data.SQlite");

		    sqlConnection = (System.Data.SQLite.SQLiteConnection) factory.CreateConnection();

		    sqlConnection.ConnectionString = "Data Source = " + dbName;
		    sqlConnection.Open();

            sqlCommand = new System.Data.SQLite.SQLiteCommand(sqlConnection);
		}

	    public void Init()
	    {
	        sqlCommand.CommandText =
	            @"CREATE TABLE IF NOT EXISTS servers (endpoint TEXT, name TEXT, gamemodes TEXT)";
	        sqlCommand.ExecuteNonQuery();
	    }
	}
}

