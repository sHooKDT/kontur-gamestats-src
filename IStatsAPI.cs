namespace Kontur.GameStats.Server
{
	public interface IStatsAPI
	{
		void GetServersInfo (System.Net.HttpListenerContext context);

		void GetServerInfo (System.Net.HttpListenerContext context);

		void PutServerInfo (System.Net.HttpListenerContext context);

		void GetServerMatch (System.Net.HttpListenerContext context);

		void PutServerMatch (System.Net.HttpListenerContext context);

		void GetServerStats (System.Net.HttpListenerContext context);

		void GetPlayerStats (System.Net.HttpListenerContext context);

		void GetRecentMatchesReport (System.Net.HttpListenerContext context);

		void GetBestPlayersReport (System.Net.HttpListenerContext context);

		void GetPopularServersReport (System.Net.HttpListenerContext context);

		void HandleIncorrect (System.Net.HttpListenerContext context);
	}
}

