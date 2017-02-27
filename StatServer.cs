using System;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Kontur.GameStats.Server
{
    public class StatServer : IDisposable
    {
        public StatServer()
        {
            listener = new HttpListener();

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

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
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
                        //Task.Run(() => this.HandleContext(context));
                        this.HandleContext(context);
                    }
                    else
                        Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Source: {error.Source}\nException: {error.Message}\nStack Trace: {error.StackTrace}");
                    Console.ResetColor();
                }
            }
        }

        private void HandleContext(HttpListenerContext listenerContext)
        {
            var request = listenerContext.Request;
            var parts = request.RawUrl.Split('/');

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{1} {0}", request.RawUrl, request.HttpMethod);
            Console.ResetColor();

            if (parts[1] == "servers")
            {
                if (parts[2] == "info")
                {
                    // /servers/info GET
                    if (request.HttpMethod == HttpMethod.Get.Method)
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
                            if (request.HttpMethod == HttpMethod.Get.Method)
                                statsApi.GetServerInfo(listenerContext);
                            else if (request.HttpMethod == HttpMethod.Put.Method)
                                statsApi.PutServerInfo(listenerContext);
                            else statsApi.HandleIncorrect(listenerContext);
                            break;
                        case "matches":
                            // /servers/<endpoint>/matches/<timestamp> PUT, GET
                            if (request.HttpMethod == HttpMethod.Get.Method)
                                statsApi.GetServerMatch(listenerContext);
                            else if (request.HttpMethod == HttpMethod.Put.Method)
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
            else if (parts[1] == "reports" && request.HttpMethod == HttpMethod.Get.Method)
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
            else if (parts[1] == "players" && parts[3] == "stats" && request.HttpMethod == HttpMethod.Get.Method)
            {
                statsApi.GetPlayerStats(listenerContext);
            }
            else
            {
                statsApi.HandleIncorrect(listenerContext);
            }
        }

        private readonly HttpListener listener;

        private readonly StatsApi statsApi;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}