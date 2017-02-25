using System.Threading.Tasks;

namespace Kontur.GameStats.Server
{
    internal class StatServer : System.IDisposable
    {
        public StatServer()
        {
            listener = new System.Net.HttpListener();

            // TODO: Database initialisation
            statsApi = new StatsApi(new DbWorker());
        }

        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new System.Threading.Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = System.Threading.ThreadPriority.Highest
                    };
                    listenerThread.Start();

                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();

                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => this.HandleContext(context));
                    }
                    else
                        System.Threading.Thread.Sleep(0);
                }
                catch (System.Threading.ThreadAbortException)
                {
                    return;
                }
                catch (System.Exception error)
                {
                    System.Console.WriteLine(error.StackTrace);
                }
            }
        }

        private void HandleContext(System.Net.HttpListenerContext listenerContext)
        {
            // TODO: implement request handling

            var request = listenerContext.Request;
            var parts = request.RawUrl.Split('/');

            System.Console.WriteLine("REQ: {0} {1}", request.RawUrl, request.HttpMethod);

            if (parts[1] == "servers")
            {
                if (parts[2] == "info")
                {
                    // /servers/info GET
                    if (request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
                        statsApi.GetServersInfo(listenerContext);
                    // If method is not get
                    else statsApi.HandleIncorrect(listenerContext);
                }
                else
                {
                    switch (parts[3])
                    {
                        case "info":
                            // /servers/<endpoint>/info PUT, GET
                            if (request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
                                statsApi.GetServerInfo(listenerContext);
                            else if (request.HttpMethod == System.Net.Http.HttpMethod.Put.Method)
                                statsApi.PutServerInfo(listenerContext);
                            else statsApi.HandleIncorrect(listenerContext);
                            break;
                        case "matches":
                            // /servers/<endpoint>/matches/<timestamp> PUT, GET
                            if (request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
                                statsApi.GetServerMatch(listenerContext);
                            else if (request.HttpMethod == System.Net.Http.HttpMethod.Put.Method)
                                statsApi.PutServerMatch(listenerContext);
                            else statsApi.HandleIncorrect(listenerContext);
                            break;
                        case "stats":
                            // /servers/<endpoint>/stats GET
                            statsApi.GetServerStats(listenerContext);
                            break;
                        default:
                            statsApi.HandleIncorrect(listenerContext);
                            break;
                    }
                }
            }
            else if (parts[1] == "reports" && request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
            {
                switch (parts[2])
                {
                    case "recent-matches":
                        statsApi.GetRecentMatchesReport(listenerContext);
                        break;
                    case "best-players":
                        statsApi.GetBestPlayersReport(listenerContext);
                        break;
                    case "popular-servers":
                        statsApi.GetPopularServersReport(listenerContext);
                        break;
                    default:
                        statsApi.HandleIncorrect(listenerContext);
                        break;
                }
            }
            else if (parts[1] == "players" && parts[3] == "stats" && request.HttpMethod == System.Net.Http.HttpMethod.Get.Method)
            {
                statsApi.GetPlayerStats(listenerContext);
            }
            else
            {
                statsApi.HandleIncorrect(listenerContext);
            }
        }

        private readonly System.Net.HttpListener listener;

        private readonly StatsApi statsApi;

        private System.Threading.Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}