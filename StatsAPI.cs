using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    public class StatsApi : IStatsApi
    {
        private readonly IDbAdapter db;

        private readonly bool enableCache;

        private readonly WeakCache<string, string> playerStatsCache;
        private readonly WeakCache<string, string> serverStatsCache;
        private readonly WeakCache<int, string> recentMatchesReportCache;
        private readonly WeakCache<int, string> bestPlayersReportCache;
        private readonly WeakCache<int, string> popularServersReportCache;

        public void GetServersInfo(HttpListenerContext context)
        {
            var servers = db.GetServersInfo();
            string serversJson = JsonConvert.SerializeObject(servers);

            this.SendResponse(context.Response, serversJson, HttpStatusCode.OK);
        }

        public void GetServerInfo(HttpListenerContext context)
        {
            string endpoint = ReqExtracters.ExtractEndpoint(context.Request);
            EndpointInfo.ServerInfo serverInfo = db.GetServerInfo(endpoint);

            if (serverInfo == null)
            {
                this.SendResponse(context.Response, "", HttpStatusCode.NotFound);
                return;
            }

            string serverInfoJson = JsonConvert.SerializeObject(serverInfo);

            this.SendResponse(context.Response, serverInfoJson, HttpStatusCode.OK);
        }

        public void PutServerInfo(HttpListenerContext context)
        {
            var inpStream = new StreamReader(context.Request.InputStream);

            EndpointInfo.ServerInfo serverInfo =
                JsonConvert.DeserializeObject<EndpointInfo.ServerInfo>(inpStream.ReadToEnd());
            string endPoint = ReqExtracters.ExtractEndpoint(context.Request);

            db.PutServerInfo(new EndpointInfo(endPoint, serverInfo));

            this.SendResponse(context.Response, "", HttpStatusCode.OK);
        }

        public void GetServerMatch(HttpListenerContext context)
        {
            string endpoint = ReqExtracters.ExtractEndpoint(context.Request);
            DateTime timestamp = ReqExtracters.ExtractTimestamp(context.Request);

            MatchInfo matchInfo = db.GetServerMatch(endpoint, timestamp);

            if (matchInfo == null)
            {
                this.SendResponse(context.Response, "", HttpStatusCode.NotFound);
                return;
            }

            string matchInfoJson = JsonConvert.SerializeObject(matchInfo);

            this.SendResponse(context.Response, matchInfoJson, HttpStatusCode.OK);
        }

        public void PutServerMatch(HttpListenerContext context)
        {
            var inpStream = new StreamReader(context.Request.InputStream);

            string endpoint = ReqExtracters.ExtractEndpoint(context.Request);
            DateTime timestamp = ReqExtracters.ExtractTimestamp(context.Request);

            MatchInfo matchInfo =
                JsonConvert.DeserializeObject<MatchInfo>(inpStream.ReadToEnd());

            this.SendResponse(context.Response, "",
                db.PutServerMatch(endpoint, timestamp, matchInfo) ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        }

        public void GetServerStats(HttpListenerContext context)
        {
            string stats = enableCache ? serverStatsCache[ReqExtracters.ExtractEndpoint(context.Request)] : db.MakeServerStats(ReqExtracters.ExtractEndpoint(context.Request));

            this.SendResponse(context.Response, stats, HttpStatusCode.OK);
        }

        public void GetPlayerStats(HttpListenerContext context)
        {
            string stats = enableCache ? playerStatsCache[ReqExtracters.ExtractName(context.Request)] : db.MakePlayerStats(ReqExtracters.ExtractName(context.Request));

            this.SendResponse(context.Response, stats, HttpStatusCode.OK);
        }

        public void GetRecentMatchesReport(HttpListenerContext context)
        {
            string report = enableCache ? recentMatchesReportCache[ReqExtracters.ExtractCount(context.Request)]: db.MakeRecentMatchesReport(ReqExtracters.ExtractCount(context.Request));

            this.SendResponse(context.Response, report, HttpStatusCode.OK);
        }

        public void GetBestPlayersReport(HttpListenerContext context)
        {
            string report = enableCache? bestPlayersReportCache[ReqExtracters.ExtractCount(context.Request)] : db.MakeBestPlayersReport(ReqExtracters.ExtractCount(context.Request)); ;

            this.SendResponse(context.Response, report, HttpStatusCode.OK);
        }

        public void GetPopularServersReport(HttpListenerContext context)
        {
            string report = enableCache ? popularServersReportCache[ReqExtracters.ExtractCount(context.Request)] : db.MakePopularServersReport(ReqExtracters.ExtractCount(context.Request));

            this.SendResponse(context.Response, report, HttpStatusCode.OK);
        }

        public void SendResponse(HttpListenerResponse response, string body, HttpStatusCode code)
        {
            // IMPORTANT: don't move this line
            // Status code must be assigned before writing

            response.StatusCode = (int)code;

            using (var writer = new StreamWriter(response.OutputStream))
            {
                writer.Write(body);
            }

            response.Close();
        }

        public void HandleIncorrect(HttpListenerContext context)
        {
            this.SendResponse(context.Response, "Incorrect", HttpStatusCode.BadRequest);
        }

        public StatsApi(IDbAdapter database, bool cacheOn)
        {
            db = database;
            enableCache = cacheOn;

            // Cache setup
            playerStatsCache = new WeakCache<string, string>(db.MakePlayerStats);
            serverStatsCache = new WeakCache<string, string>(db.MakeServerStats);
            recentMatchesReportCache = new WeakCache<int, string>(db.MakeRecentMatchesReport);
            bestPlayersReportCache = new WeakCache<int, string>(db.MakeBestPlayersReport);
            popularServersReportCache = new WeakCache<int, string>(db.MakePopularServersReport);
        }
    }
}