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

            context.Response.StatusCode = (int) HttpStatusCode.Accepted;
            using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
            {
                writer.WriteLine(serversJson);
            }
        }

        public void GetServerInfo(HttpListenerContext context)
        {
            string endpoint = ExtractEndpoint(context.Request);
            EndpointInfo.ServerInfo serverInfo = db.GetServerInfo(endpoint);

            if (serverInfo == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return;
            }

            string serverInfoJson = JsonConvert.SerializeObject(serverInfo);

            context.Response.StatusCode = (int) HttpStatusCode.OK;
            using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
            {
                writer.Write(serverInfoJson);
            }
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
            string timestamp = context.Request.RawUrl.Split('/')[4];

            MatchInfo matchInfo = db.GetServerMatch(endpoint, timestamp);
            string matchInfoJson = JsonConvert.SerializeObject(matchInfo);

            context.Response.StatusCode = (int) HttpStatusCode.OK;
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(matchInfoJson);
            }
        }

        public void PutServerMatch(HttpListenerContext context)
        {
            var inpStream = new StreamReader(context.Request.InputStream);

            MatchInfo matchInfo =
                JsonConvert.DeserializeObject<MatchInfo>(inpStream.ReadToEnd());

            db.PutServerMatch(/* endpoint, timestamp,*/matchInfo);
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
        }

        private static string ExtractEndpoint(HttpListenerRequest req)
        {
            return req.RawUrl.Split('/')[2];
        }

        public StatsApi(IDbWorker database)
        {
            db = database;
        }
    }
}