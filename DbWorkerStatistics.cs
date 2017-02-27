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
                {"totalMatchesPlayed", "SELECT count(*) FROM scoreboard WHERE name = \"{0}\""},
                {
                    "totalMatchesWon",
                    "SELECT count(*) FROM (SELECT match_id, name, max(frags) FROM scoreboard GROUP BY match_id) WHERE name = \"{0}\" COLLATE NOCASE"
                },
                {"favouriteServer", ""},
                {"uniqueServers", ""},
                {"favouriteGameMode", ""},
                {"averageScoreboardPercent", ""},
                {"maximumMatchesPerDay", ""},
                {"averageMatchesPerDay", ""},
                {"lastMatchPlayed", ""},
                {"killToDeathRatio", ""}
            };


            var stats = new JObject
            {
                // ...
            };


            return stats.ToString();
        }
    }
}