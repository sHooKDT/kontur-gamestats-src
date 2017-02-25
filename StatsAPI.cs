using System;
using System.Net;

namespace Kontur.GameStats.Server
{
	public class StatsApi : IStatsApi
	{
		private readonly DbWorker db;

		public void GetServersInfo (HttpListenerContext context)
		{
		    context.Response.StatusCode = (int) HttpStatusCode.Accepted;
		    using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
		    {
		        writer.WriteLine("You are now getting servers info !?");
		    }
		}

		public void GetServerInfo (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void PutServerInfo (HttpListenerContext context)
		{
		    db.PutServerInfo("example", "example", "example");

		    context.Response.StatusCode = (int) HttpStatusCode.Accepted;
            using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
            {
                writer.WriteLine("put something or not, who knows..");
            }
        }

		public void GetServerMatch (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void PutServerMatch (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void GetServerStats (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void GetPlayerStats (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void GetRecentMatchesReport (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void GetBestPlayersReport (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void GetPopularServersReport (HttpListenerContext context)
		{
			throw new NotImplementedException ();
		}

		public void HandleIncorrect (HttpListenerContext context)
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

		public StatsApi (DbWorker database)
		{
			db = database;
		}
	}
}

