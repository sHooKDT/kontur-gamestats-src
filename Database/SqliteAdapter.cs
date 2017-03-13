using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Kontur.GameStats.Server
{
    public partial class SqliteAdapter : IDbAdapter
    {
        private readonly SQLiteConnection sqlConnection;
        private readonly SQLiteCommand sqlCommand;
        private const string DbName = "GameStats.sqlite3";

        public SqliteAdapter()
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
            string[] tablesCreation = new[]
            {
                @"servers (endpoint TEXT PRIMARY KEY, name TEXT, gamemodes TEXT)",
                @"matches (id INTEGER PRIMARY KEY, endpoint TEXT, timestamp INTEGER, map TEXT, gamemode TEXT, frag_limit INTEGER, time_limit INTEGER, time_elapsed REAL, UNIQUE(endpoint, timestamp) ON CONFLICT IGNORE)",
                @"scoreboard (match_id INTEGER, name TEXT, frags INTEGER, kills INTEGER, deaths INTEGER, UNIQUE (match_id, name) ON CONFLICT IGNORE)"
            };

            foreach (string table in tablesCreation)
            {
                sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS " + table;
                this.PrintSqlQuery();
                sqlCommand.ExecuteNonQuery();
            }
        }

        public IEnumerable<EndpointInfo> GetServersInfo()
        {
            sqlCommand.CommandText = "SELECT * FROM servers;";
            this.PrintSqlQuery();

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                while (reader.Read())
                {
                    string endpoint = (string) reader["endpoint"];
                    string name = (string) reader["name"];
                    string gamemodes = (string) reader["gamemodes"];

                    yield return new EndpointInfo(endpoint, new EndpointInfo.ServerInfo(name, gamemodes.Split(',')));
                }
        }

        public EndpointInfo.ServerInfo GetServerInfo(string endpoint)
        {
            sqlCommand.CommandText = $"SELECT name, gamemodes FROM servers WHERE endpoint = \"{endpoint}\";";
            this.PrintSqlQuery();

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                if (reader.Read())
                    return new EndpointInfo.ServerInfo(reader.GetString(0), reader.GetString(1).Split(','));

            return null;
        }

        public bool PutServerInfo(EndpointInfo server)
        {
            sqlCommand.CommandText =
                $"INSERT OR REPLACE INTO servers VALUES (\"{server.endpoint}\", \"{server.info.name}\", \"{server.info.GetGameModesString()}\");";
            this.PrintSqlQuery();

            return sqlCommand.ExecuteNonQuery() > 0;
        }

        public MatchInfo GetServerMatch(string endpoint, DateTime timestamp)
        {
            double unixTimestamp = Extras.DateTimeToUnixTime(timestamp);

            sqlCommand.CommandText =
                $"SELECT * FROM matches WHERE endpoint = \"{endpoint}\" AND timestamp = {unixTimestamp};";
            this.PrintSqlQuery();

            MatchInfo matchInfo;
            int matchId = 0;

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    matchId = (int) (long) reader["id"];

                    matchInfo = new MatchInfo()
                    {
                        map = (string) reader["map"],
                        gameMode = (string) reader["gamemode"],
                        fragLimit = (int) (long) reader["frag_limit"],
                        timeLimit = (int) (long) reader["time_limit"],
                        timeElapsed = (double) reader["time_elapsed"]
                    };
                }
                else
                {
                    return null;
                }
            }

            matchInfo.scoreboard = this.GetScoreboard(matchId).ToArray();

            return matchInfo;
        }

        public IEnumerable<MatchInfo.ScoreboardItem> GetScoreboard(int matchId)
        {
            sqlCommand.CommandText = $"SELECT * FROM scoreboard WHERE match_id = {matchId} ORDER BY frags DESC";
            this.PrintSqlQuery();
            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return new MatchInfo.ScoreboardItem()
                    {
                        name = (string) reader["name"],
                        deaths = (int) (long) reader["deaths"],
                        frags = (int) (long) reader["frags"],
                        kills = (int) (long) reader["kills"]
                    };
                }
            }
        }

        public bool PutServerMatch(string endpoint, DateTime timestamp, MatchInfo match)
        {
            double unixTimestamp = Extras.DateTimeToUnixTime(timestamp);

            sqlCommand.CommandText =
                "INSERT INTO matches (endpoint, timestamp, map, gamemode, frag_limit, time_limit, time_elapsed) " +
                $"VALUES (\"{endpoint}\", {unixTimestamp}, \"{match.map}\", \"{match.gameMode}\"," +
                $" {match.fragLimit}, {match.timeLimit}, {match.timeElapsed.ToString(CultureInfo.InvariantCulture)})";
            this.PrintSqlQuery();

            int affectedRows = sqlCommand.ExecuteNonQuery();

            int addedMatchId = (int) sqlConnection.LastInsertRowId;

            if (affectedRows != 0)
            {
                var sb = new StringBuilder();
                sb.Append("INSERT INTO scoreboard (match_id, name, frags, kills, deaths) \n");
                sb.Append("SELECT 0 as match_id, \"0\" as name, 0 as frags, 0 as kills, 0 as deaths \n");
                foreach (var item in match.scoreboard)
                {
                    sb.AppendLine("UNION SELECT " + addedMatchId + "," + item.ToString());
                }
                sqlCommand.CommandText = sb.ToString();
                this.PrintSqlQuery();
                affectedRows += sqlCommand.ExecuteNonQuery();
            }

            return affectedRows > 0;
        }

        public int GetOneInt(string query, string param)
        {
            sqlCommand.CommandText = string.Format(query, param);
            this.PrintSqlQuery();
            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                if (reader.Read()) return reader.GetInt32(0);
            }
            throw new Exception("No data");
        }

        public double GetOneDouble(string query, string param)
        {
            sqlCommand.CommandText = string.Format(query, param);
            this.PrintSqlQuery();
            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                if (reader.Read()) return reader.GetDouble(0);
            }
            throw new Exception("No data");
        }

        public IEnumerable<string> GetStringArray(string query, string param)
        {
            sqlCommand.CommandText = string.Format(query, param);
            this.PrintSqlQuery();
            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read()) yield return reader.GetString(0);
            }
        }

        public void PrintSqlQuery()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(sqlCommand.CommandText);
            Console.ResetColor();
        }
    }
}