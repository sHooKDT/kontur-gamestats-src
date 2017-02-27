using System;
using System.IO;
using System.Net;
using Kontur.GameStats.Server.Datatypes;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    public class StatsApi : IStatsApi
    {
        private readonly IDbWorker db;

        public void GetServersInfo(HttpListenerContext context)
        {

            EndpointInfo[] servers = db.GetServersInfo();
            string serversJson = JsonConvert.SerializeObject(servers);

            using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
            {
                writer.WriteLine(serversJson);
            }
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            context.Response.Close();
        }

        public void GetServerInfo(HttpListenerContext context)
        {
            string endpoint = ExtractEndpoint(context.Request);
            EndpointInfo.ServerInfo serverInfo = db.GetServerInfo(endpoint);

            if (serverInfo == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            }

            string serverInfoJson = JsonConvert.SerializeObject(serverInfo);

            using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
            {
                writer.Write(serverInfoJson);
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
        }

        public void PutServerInfo(HttpListenerContext context)
        {
            var inpStream = new StreamReader(context.Request.InputStream);

            EndpointInfo.ServerInfo serverInfo =
                JsonConvert.DeserializeObject<EndpointInfo.ServerInfo>(inpStream.ReadToEnd());
            string endPoint = ExtractEndpoint(context.Request);

            db.PutServerInfo(new EndpointInfo(endPoint, serverInfo));

            context.Response.StatusCode = (int) HttpStatusCode.OK;
            context.Response.Close();
        }

        public void GetServerMatch(HttpListenerContext context)
        {
            string endpoint = ExtractEndpoint(context.Request);
            DateTime timestamp = ExtractTimestamp(context.Request);

            MatchInfo matchInfo = db.GetServerMatch(endpoint, timestamp);

            if (matchInfo == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                context.Response.Close();
                return;
            } 

            string matchInfoJson = JsonConvert.SerializeObject(matchInfo);

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(matchInfoJson);
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Close();
        }

        public void PutServerMatch(HttpListenerContext context)
        {
            var inpStream = new StreamReader(context.Request.InputStream);

            string endpoint = ExtractEndpoint(context.Request);
            DateTime timestamp = ExtractTimestamp(context.Request);

            MatchInfo matchInfo =
                JsonConvert.DeserializeObject<MatchInfo>(inpStream.ReadToEnd());

            if (db.PutServerMatch(endpoint, timestamp, matchInfo) == true)
                context.Response.StatusCode = (int) HttpStatusCode.OK;
            else context.Response.StatusCode = (int) HttpStatusCode.BadRequest;

            context.Response.Close();
        }

        public void GetServerStats(HttpListenerContext context)
        {
            throw new NotImplementedException();
        }

        public void GetPlayerStats(HttpListenerContext context)
        {
            throw new NotImplementedException();
        }

        public void GetRecentMatchesReport(HttpListenerContext context)
        {
            throw new NotImplementedException();
        }

        public void GetBestPlayersReport(HttpListenerContext context)
        {
            throw new NotImplementedException();
        }

        public void GetPopularServersReport(HttpListenerContext context)
        {
            throw new NotImplementedException();
        }

        public void HandleIncorrect(HttpListenerContext context)
        {
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;

            var parts = context.Request.RawUrl.Split('/');

            Console.WriteLine("Incorrect \n" +
                              $"1: {parts[1]} \n" +
                              $"2: {parts[2]} \n" +
                              $"3: {parts[3] ?? "null"}");

            using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
            {
                writer.WriteLine("400 - Incorrect request");
            }

            context.Response.Close();
        }

        private static string ExtractEndpoint(HttpListenerRequest req)
        {
            return req.RawUrl.Split('/')[2];
        }

        private static DateTime ExtractTimestamp(HttpListenerRequest req)
        {
            return DateTimeOffset.Parse(req.RawUrl.Split('/')[4]).UtcDateTime;
        }

        private static int ExtractCount(HttpListenerRequest req)
        {
            string[] spl = req.RawUrl.Split('/');
            // If count isn't set, default value = 5
            if (spl.Length < 3) return 5;

            int count = int.Parse(spl[3]);
            
            // 50, no more
            if (count >= 50) return 50;
            // 0, no less
            if (count <= 0) return 0;

            return count;
        }

        public StatsApi(IDbWorker database)
        {
            db = database;
        }
    }
}