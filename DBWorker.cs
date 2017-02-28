using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using Kontur.GameStats.Server.Datatypes;

namespace Kontur.GameStats.Server
{
    public partial class DbWorker : IDbWorker
    {
        private readonly SQLiteConnection sqlConnection;
        private readonly SQLiteCommand sqlCommand;
        private const string DbName = "GameStats.sqlite3";

        public DbWorker()
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
                @"matches (id INTEGER PRIMARY KEY, endpoint TEXT, timestamp INTEGER, map TEXT, gamemode TEXT, frag_limit INTEGER, time_limit INTEGER, time_elapsed REAL)",
                @"scoreboard (match_id INTEGER, name TEXT, frags INTEGER, kills INTEGER, deaths INTEGER)"
            };

            foreach (string table in tablesCreation)
            {
                sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS " + table;
                sqlCommand.ExecuteNonQuery();
            }
        }

        public EndpointInfo[] GetServersInfo()
        {
            var servers = new List<EndpointInfo>();

            sqlCommand.CommandText = "SELECT * FROM servers;";
            this.PrintSqlQuery();

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                while (reader.Read())
                {
                    string endpoint = (string) reader["endpoint"];
                    string name = (string) reader["name"];
                    string gamemodes = (string) reader["gamemodes"];

                    // TODO: Simplify with yield return
                    servers.Add(new EndpointInfo(endpoint, new EndpointInfo.ServerInfo(name, gamemodes.Split(','))));
                }

            return servers.ToArray();
        }

        public EndpointInfo.ServerInfo GetServerInfo(string endpoint)
        {
            EndpointInfo.ServerInfo serverInfo = null;

            sqlCommand.CommandText = $"SELECT name, gamemodes FROM servers WHERE endpoint = \"{endpoint}\";";
            this.PrintSqlQuery();

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
                while (reader.Read())
                {
                    serverInfo = new EndpointInfo.ServerInfo(reader.GetString(0), reader.GetString(1).Split(','));
                    break;
                }

            return serverInfo;
        }

        public bool PutServerInfo(EndpointInfo server)
        {
            sqlCommand.CommandText =
                $"INSERT OR REPLACE INTO servers VALUES (\"{server.endpoint}\", \"{server.info.name}\", \"{server.info.GetGameModesString()}\");";
            this.PrintSqlQuery();

            int affected = sqlCommand.ExecuteNonQuery();
            return affected > 0;
        }

        public MatchInfo GetServerMatch(string endpoint, DateTime timestamp)
        {
            double unixTimestamp = timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

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
            // TODO: Don't add not unique matches
            double unixTimestamp = timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            sqlCommand.CommandText =
                "INSERT INTO matches (endpoint, timestamp, map, gamemode, frag_limit, time_limit, time_elapsed) " +
                $"VALUES (\"{endpoint}\", {unixTimestamp}, \"{match.map}\", \"{match.gameMode}\", {match.fragLimit}, {match.timeLimit}, {match.timeElapsed.ToString(CultureInfo.InvariantCulture)})";
            this.PrintSqlQuery();

            int affectedRows = sqlCommand.ExecuteNonQuery();

            int addedMatchId = (int) sqlConnection.LastInsertRowId;

            if (affectedRows != 0)
                foreach (MatchInfo.ScoreboardItem line in match.scoreboard)
                {
                    sqlCommand.CommandText =
                        $"INSERT INTO scoreboard (match_id, name, frags, kills, deaths) VALUES ({addedMatchId}, \"{line.name}\", {line.frags}, {line.kills}, {line.deaths});";
                    this.PrintSqlQuery();
                    affectedRows += sqlCommand.ExecuteNonQuery();
                }

            return affectedRows > 0;
        }

        public int GetOneInt (/*SQLiteCommand command,*/ string query, string param)
        {
            sqlCommand.CommandText = string.Format(query, param);
            this.PrintSqlQuery();
            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                if (reader.Read()) return reader.GetInt32(0);
            }
            throw new Exception("No data");
        }

        public double GetOneDouble(/*SQLiteCommand command,*/ string query, string param)
        {
            sqlCommand.CommandText = string.Format(query, param);
            this.PrintSqlQuery();
            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                if (reader.Read()) return reader.GetDouble(0);
            }
            throw new Exception("No data");
        }

        public IEnumerable<string> GetStringArray(/*SQLiteCommand command,*/ string query, string param)
        {
            sqlCommand.CommandText = string.Format(query, param);
            this.PrintSqlQuery();
            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read()) yield return reader.GetString(0);
            }
        }

        private void PrintSqlQuery()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(sqlCommand.CommandText);
            Console.ResetColor();
        }
    }
}