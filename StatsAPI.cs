namespace Kontur.GameStats.Server
{
	public class StatsAPI : IStatsAPI
	{
		private readonly DBWorker db;

		public void GetServersInfo (System.Net.HttpListenerContext context)
		{
		    context.Response.StatusCode = (int) System.Net.HttpStatusCode.Accepted;
		    using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
		    {
		        writer.WriteLine("You are now getting servers info !?");
		    }
		}

		public void GetServerInfo (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void PutServerInfo (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void GetServerMatch (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void PutServerMatch (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void GetServerStats (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void GetPlayerStats (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void GetRecentMatchesReport (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void GetBestPlayersReport (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void GetPopularServersReport (System.Net.HttpListenerContext context)
		{
			throw new System.NotImplementedException ();
		}

		public void HandleIncorrect (System.Net.HttpListenerContext context)
		{
		    context.Response.StatusCode = (int) System.Net.HttpStatusCode.BadRequest;

		    using (var writer = new System.IO.StreamWriter(context.Response.OutputStream))
		    {
		        writer.WriteLine("400 - Incorrect request");
		    }
		}

		public StatsAPI (DBWorker database)
		{
			db = database;
		}
	}
}

