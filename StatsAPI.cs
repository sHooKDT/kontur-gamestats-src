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
        private readonly IDbAdapter _db;

        private readonly bool _enableCache;

        private readonly WeakCache<string, string> _playerStatsCache;
        private readonly WeakCache<string, string> _serverStatsCache;
        private readonly WeakCache<int, string> _recentMatchesReportCache;
        private readonly WeakCache<int, string> _bestPlayersReportCache;
        private readonly WeakCache<int, string> _popularServersReportCache;

        public void GetServersInfo(HttpListenerContext context)
        {
            var servers = _db.GetServersInfo();
            string serversJson = JsonConvert.SerializeObject(servers);

            this.SendResponse(context.Response, serversJson, HttpStatusCode.OK);
        }

        public void GetServerInfo(HttpListenerContext context)
        {
            string endpoint = ReqExtracters.ExtractEndpoint(context.Request);
            EndpointInfo.ServerInfo serverInfo = _db.GetServerInfo(endpoint);

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

            _db.PutServerInfo(new EndpointInfo(endPoint, serverInfo));

            this.SendResponse(context.Response, "", HttpStatusCode.OK);
        }

        public void GetServerMatch(HttpListenerContext context)
        {
            string endpoint = ReqExtracters.ExtractEndpoint(context.Request);
            DateTime timestamp = ReqExtracters.ExtractTimestamp(context.Request);

            MatchInfo matchInfo = _db.GetServerMatch(endpoint, timestamp);

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
                _db.PutServerMatch(endpoint, timestamp, matchInfo) ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        }

        public void GetServerStats(HttpListenerContext context)
        {
            string stats = _enableCache ? _serverStatsCache[ReqExtracters.ExtractEndpoint(context.Request)] : _db.MakeServerStats(ReqExtracters.ExtractEndpoint(context.Request));

            this.SendResponse(context.Response, stats, HttpStatusCode.OK);
        }

        public void GetPlayerStats(HttpListenerContext context)
        {
            string stats = _enableCache ? _playerStatsCache[ReqExtracters.ExtractName(context.Request)] : _db.MakePlayerStats(ReqExtracters.ExtractName(context.Request));

            this.SendResponse(context.Response, stats, HttpStatusCode.OK);
        }

        public void GetRecentMatchesReport(HttpListenerContext context)
        {
            string report = _enableCache ? _recentMatchesReportCache[ReqExtracters.ExtractCount(context.Request)]: _db.MakeRecentMatchesReport(ReqExtracters.ExtractCount(context.Request));

            this.SendResponse(context.Response, report, HttpStatusCode.OK);
        }

        public void GetBestPlayersReport(HttpListenerContext context)
        {
            string report = _enableCache? _bestPlayersReportCache[ReqExtracters.ExtractCount(context.Request)] : _db.MakeBestPlayersReport(ReqExtracters.ExtractCount(context.Request)); ;

            this.SendResponse(context.Response, report, HttpStatusCode.OK);
        }

        public void GetPopularServersReport(HttpListenerContext context)
        {
            string report = _enableCache ? _popularServersReportCache[ReqExtracters.ExtractCount(context.Request)] : _db.MakePopularServersReport(ReqExtracters.ExtractCount(context.Request));

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
            _db = database;
            _enableCache = cacheOn;

            // Cache setup
            _playerStatsCache = new WeakCache<string, string>(_db.MakePlayerStats);
            _serverStatsCache = new WeakCache<string, string>(_db.MakeServerStats);
            _recentMatchesReportCache = new WeakCache<int, string>(_db.MakeRecentMatchesReport);
            _bestPlayersReportCache = new WeakCache<int, string>(_db.MakeBestPlayersReport);
            _popularServersReportCache = new WeakCache<int, string>(_db.MakePopularServersReport);
        }
    }
}