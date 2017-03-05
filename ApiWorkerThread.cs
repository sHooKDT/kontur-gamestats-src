using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Kontur.GameStats.Server
{
    public class ApiWorkerThread
    {
        private readonly Thread _thread;
        private readonly StatsApi _api;
        private readonly Queue<HttpListenerContext> _queue;
        private readonly ManualResetEvent _stop, _ready;

        public ApiWorkerThread(StatsApi api, Queue<HttpListenerContext> queue, ManualResetEvent ready, ManualResetEvent stop)
        {
            _api = api;
            _queue = queue;
            _ready = ready;
            _stop = stop;
            this._thread = new Thread(this.Worker);
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Join()
        {
            _thread.Join();
        }

        private void Worker()
        {
            WaitHandle[] wait = {_ready, _stop};
            while (0 == WaitHandle.WaitAny(wait))
            {
                HttpListenerContext context;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try
                {
                    //ProcessRequest(context);
                    this.HandleContext(context);
                }
                catch (ArgumentException)
                {
                    Extras.WriteColoredLine("Incorrect request", ConsoleColor.Magenta);
                    _api.HandleIncorrect(context);
                }

                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    _api.HandleIncorrect(context);
                    Extras.WriteColoredLine(
                        $"Source: {error.Source}\nException: {error.Message}\nStack Trace: {error.StackTrace}",
                        ConsoleColor.Red);
                }
            }
        }

        private void HandleContext(HttpListenerContext listenerContext)
        {
            // TODO: Make routing with regexp
            var request = listenerContext.Request;
            var parts = request.RawUrl.Split('/');

            Extras.WriteColoredLine(String.Format("{1} {0}", request.RawUrl, request.HttpMethod), ConsoleColor.DarkGreen);

            if (parts[1] == "servers")
            {
                if (parts[2] == "info")
                {
                    // /servers/info GET
                    if (request.HttpMethod == HttpMethod.Get.Method)
                        _api.GetServersInfo(listenerContext);
                    // If method is not get
                    else _api.HandleIncorrect(listenerContext);
                }
                else
                {
                    switch (parts[3])
                    {
                        case "info":
                            // /servers/<endpoint>/info PUT, GET
                            if (request.HttpMethod == HttpMethod.Get.Method)
                                _api.GetServerInfo(listenerContext);
                            else if (request.HttpMethod == HttpMethod.Put.Method)
                                _api.PutServerInfo(listenerContext);
                            else _api.HandleIncorrect(listenerContext);
                            break;
                        case "matches":
                            // /servers/<endpoint>/matches/<timestamp> PUT, GET
                            if (request.HttpMethod == HttpMethod.Get.Method)
                                _api.GetServerMatch(listenerContext);
                            else if (request.HttpMethod == HttpMethod.Put.Method)
                                _api.PutServerMatch(listenerContext);
                            else _api.HandleIncorrect(listenerContext);
                            break;
                        case "stats":
                            // /servers/<endpoint>/stats GET
                            _api.GetServerStats(listenerContext);
                            break;
                        default:
                            _api.HandleIncorrect(listenerContext);
                            break;
                    }
                }
            }
            else if (parts[1] == "reports" && request.HttpMethod == HttpMethod.Get.Method)
            {
                switch (parts[2])
                {
                    case "recent-matches":
                        _api.GetRecentMatchesReport(listenerContext);
                        break;
                    case "best-players":
                        _api.GetBestPlayersReport(listenerContext);
                        break;
                    case "popular-servers":
                        _api.GetPopularServersReport(listenerContext);
                        break;
                    default:
                        _api.HandleIncorrect(listenerContext);
                        break;
                }
            }
            else if (parts[1] == "players" && parts[3] == "stats" && request.HttpMethod == HttpMethod.Get.Method)
            {
                _api.GetPlayerStats(listenerContext);
            }
            else
            {
                _api.HandleIncorrect(listenerContext);
            }
        }
    }
}
