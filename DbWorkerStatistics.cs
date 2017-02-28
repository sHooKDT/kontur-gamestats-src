using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Kontur.GameStats.Server
{
    public partial class DbWorker
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
            var statsRequests = new Dictionary<string, string>()
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
                // TODO: Calculate average scoreboard percent
                {"averageScoreboardPercent", ""},
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
                {"totalMatchesPlayed", this.GetOneInt(statsRequests["totalMatchesPlayed"], name)},
                {"totalMatchesWon", this.GetOneInt(statsRequests["totalMatchesWon"], name)},
                {"favouriteServer", this.GetStringArray(statsRequests["favouriteServer"], name).ToArray()[0]},
                {"uniqueServers", this.GetOneInt(statsRequests["uniqueServers"], name)},
                {"favouriteGameMode", this.GetStringArray(statsRequests["favouriteGameMode"], name).ToArray()[0]},
                {"maximumMatchesPerDay", this.GetOneInt(statsRequests["maximumMatchesPerDay"], name)},
                {"averageMatchesPerDay", this.GetOneDouble(statsRequests["averageMatchesPerDay"], name)},
                {
                    "lastMatchPlayed",
                    UnixTimeStampToDateTime(this.GetOneDouble(statsRequests["lastMatchPlayed"], name)).ToUniversalTime()
                }
            };


            return stats.ToString();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}