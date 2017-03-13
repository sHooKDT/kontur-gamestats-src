using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kontur.GameStats.Server
{
    public partial class SqliteAdapter
    {
        public string MakeServerStats(string endpoint)
        {
            var statsRequests = new Dictionary<string, string>()
            {
                {"totalMatchesPlayed", "SELECT count(*) FROM matches WHERE endpoint = \"{0}\""},
                {
                    "maximumMatchesPerDay",
                    "SELECT max(cnt) FROM (SELECT count(*) AS cnt FROM matches WHERE endpoint = \"{0}\" GROUP BY timestamp / 86400)"
                },
                {
                    "averageMatchesPerDay",
                    "SELECT avg(cnt) FROM (SELECT count(*) AS cnt FROM matches WHERE endpoint = \"{0}\" GROUP BY timestamp / 86400)"
                },
                {
                    "maximumPopulation",
                    "SELECT max(cnt) FROM (SELECT count(*) as cnt FROM scoreboard WHERE match_id IN (SELECT id FROM matches WHERE endpoint = \"{0}\") GROUP BY match_id)"
                },
                {
                    "averagePopulation",
                    "SELECT avg(cnt) FROM (SELECT count(*) as cnt FROM scoreboard WHERE match_id IN (SELECT id FROM matches WHERE endpoint = \"{0}\") GROUP BY match_id)"
                },
                {
                    "top5GameModes",
                    "SELECT gamemode FROM matches WHERE endpoint = \"{0}\" GROUP BY gamemode ORDER BY count(*) DESC LIMIT 5"
                },
                {
                    "top5Maps",
                    "SELECT map FROM matches WHERE endpoint = \"{0}\" GROUP BY map ORDER BY count(*) DESC LIMIT 5"
                }
            };


            var stats = new JObject
            {
                {"totalMatchesPlayed", this.GetOneInt(statsRequests["totalMatchesPlayed"], endpoint)},
                {"maximumMatchesPerDay", this.GetOneInt(statsRequests["maximumMatchesPerDay"], endpoint)},
                {"averageMatchesPerDay", this.GetOneDouble(statsRequests["averageMatchesPerDay"], endpoint)},
                {"maximumPopulation", this.GetOneInt(statsRequests["maximumPopulation"], endpoint)},
                {"averagePopulation", this.GetOneDouble(statsRequests["averagePopulation"], endpoint)},
                {"top5GameModes", new JArray(this.GetStringArray(statsRequests["top5GameModes"], endpoint))},
                {"top5Maps", new JArray(this.GetStringArray(statsRequests["top5Maps"], endpoint))}
            };


            return stats.ToString();
        }

        public string MakePlayerStats(string name)
        {
            var queries = new Dictionary<string, string>()
            {
                {"totalMatchesPlayed", "SELECT count(*) FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE"},
                {
                    "totalMatchesWon",
                    "SELECT count(*) FROM (SELECT match_id, name, max(frags) FROM scoreboard GROUP BY match_id) WHERE name = \"{0}\" COLLATE NOCASE"
                },
                {
                    "favouriteServer",
                    "SELECT endpoint FROM (SELECT endpoint, count(*) AS cnt FROM matches WHERE id IN (SELECT match_id FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE) GROUP BY endpoint ORDER BY cnt DESC LIMIT 1)"
                },
                {
                    "uniqueServers",
                    "SELECT count(*) FROM (SELECT endpoint, count(*) AS cnt FROM matches WHERE id IN (SELECT match_id FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE) GROUP BY endpoint)"
                },
                {
                    "favouriteGameMode",
                    "SELECT gamemode FROM (SELECT gamemode, count(*) AS cnt FROM matches WHERE id IN (SELECT match_id FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE) GROUP BY gamemode ORDER BY cnt DESC LIMIT 1)"
                },
                {
                    "averageScoreboardPercent",
                    "SELECT avg(sp) FROM (SELECT match_id, bp * 100.0 / (tp - 1) AS sp FROM (SELECT match_id, count(*) AS bp, tp FROM scoreboard JOIN ( SELECT match_id AS mi, frags AS target_frags FROM scoreboard WHERE name = \"{0}\" ) ON scoreboard.match_id = mi JOIN ( SELECT match_id AS mid, count(*) AS tp FROM scoreboard GROUP BY match_id ) ON scoreboard.match_id = mid WHERE match_id IN (SELECT match_id FROM scoreboard WHERE name = \"{0}\" ) AND frags < target_frags GROUP BY match_id))"
                },
                {
                    "maximumMatchesPerDay",
                    "SELECT max(cnt) FROM ( SELECT timestamp/86400 AS day, count(*) AS cnt FROM matches WHERE id IN (SELECT match_id FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE) GROUP BY day)"
                },
                {
                    "averageMatchesPerDay",
                    "SELECT avg(cnt) FROM ( SELECT timestamp/86400 AS day, count(*) AS cnt FROM matches WHERE id IN (SELECT match_id FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE) GROUP BY day)"
                },
                {
                    "lastMatchPlayed",
                    "SELECT max(timestamp) FROM matches WHERE id IN (SELECT match_id FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE)"
                },
                {
                    "killToDeathRatio",
                    "SELECT sum(kills) * 1.0 /  sum(deaths) FROM scoreboard WHERE name = \"{0}\" COLLATE NOCASE"
                }
            };


            var stats = new JObject
            {
                {"totalMatchesPlayed", this.GetOneInt(queries["totalMatchesPlayed"], name)},
                {"totalMatchesWon", this.GetOneInt(queries["totalMatchesWon"], name)},
                {"favouriteServer", this.GetStringArray(queries["favouriteServer"], name).ToArray()[0]},
                {"uniqueServers", this.GetOneInt(queries["uniqueServers"], name)},
                {"favouriteGameMode", this.GetStringArray(queries["favouriteGameMode"], name).ToArray()[0]},
                {"averageScoreboardPercent", this.GetOneDouble(queries["averageScoreboardPercent"], name)},
                {"maximumMatchesPerDay", this.GetOneInt(queries["maximumMatchesPerDay"], name)},
                {"averageMatchesPerDay", this.GetOneDouble(queries["averageMatchesPerDay"], name)},
                {
                    "lastMatchPlayed",
                    Extras.UnixTimeToDateTime(this.GetOneDouble(queries["lastMatchPlayed"], name)).ToUniversalTime()
                },
                {"killToDeathRatio", this.GetOneDouble(queries["killToDeathRatio"], name)}
            };


            return stats.ToString();
        }

        public string MakeRecentMatchesReport(int count)
        {
            sqlCommand.CommandText = $"SELECT * FROM matches ORDER BY timestamp DESC LIMIT {count}";
            this.PrintSqlQuery();

            var recentMatchesReport = new Dictionary<int, JObject>();

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var matchInfo = new JObject
                    {
                        {"server", (string) reader["endpoint"]},
                        {"timestamp", Extras.UnixTimeToDateTime((double) (long) reader["timestamp"]).ToUniversalTime()},
                        {
                            "results", new JObject
                            {
                                {"map", (string) reader["map"]},
                                {"gameMode", (string) reader["gamemode"]},
                                {"fragLimit", (int) (long) reader["frag_limit"]},
                                {"timeLimit", (int) (long) reader["time_limit"]},
                                {"timeElapsed", (double) reader["time_elapsed"]},
                                {"scoreboard", new JArray()}
                            }
                        }
                    };
                    recentMatchesReport.Add((int) (long) reader["id"], matchInfo);
                }
            }

            var recentMatchesJson = new JArray();

            foreach (var match in recentMatchesReport)
            {
                // TODO: Fix converting to json from scoreboardItem
                match.Value["results"]["scoreboard"] =
                    new JArray(
                        this.GetScoreboard(match.Key)
                            .Select(a => JsonConvert.DeserializeObject(JsonConvert.SerializeObject(a))));
                recentMatchesJson.Add(match.Value);
            }

            return recentMatchesJson.ToString();
        }

        public string MakeBestPlayersReport(int count)
        {
            sqlCommand.CommandText =
                $"SELECT name, kills * 1.0 / deaths AS kda FROM (SELECT name, sum(kills) as kills, sum(deaths) as deaths, count(*) AS played FROM scoreboard GROUP BY name COLLATE NOCASE) WHERE played > 10 AND deaths > 0 ORDER BY kda DESC LIMIT {count}";
            this.PrintSqlQuery();
            var bestPlayersReport = new JArray();

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    bestPlayersReport.Add(new JObject
                    {
                        {"name", (string) reader["name"]},
                        {"killToDeathRatio", (double) reader["kda"]}
                    });
                }
            }

            return bestPlayersReport.ToString();
        }

        public string MakePopularServersReport(int count)
        {
            sqlCommand.CommandText =
                $"SELECT name, endpoint, avg(cnt) AS pop FROM (" +
                $"SELECT *, count(*) AS cnt FROM (" +
                $"SELECT servers.name, matches.endpoint, matches.timestamp / 86400 AS day " +
                $"FROM matches JOIN servers ON matches.endpoint = servers.endpoint\r\n) GROUP BY endpoint, day" +
                $") GROUP BY endpoint ORDER BY pop DESC LIMIT {count}";
            this.PrintSqlQuery();
            var popularServersReport = new JArray();

            using (SQLiteDataReader reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    popularServersReport.Add(new JObject
                    {
                        {"endpoint", (string) reader["endpoint"]},
                        {"name", (string) reader["name"]},
                        {"averageMatchesPerDay", (double) reader["pop"]}
                    });
                }
            }

            return popularServersReport.ToString();
        }
    }
}