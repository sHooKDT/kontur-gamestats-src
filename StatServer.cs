using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace Kontur.GameStats.Server
{
    public class StatServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly StatsApi _statsApi;

        private readonly Thread _listenerThread;
        //private readonly Thread[] _workers;
        private readonly ApiWorkerThread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private readonly Queue<HttpListenerContext> _queue;

        public StatServer(int maxThreads)
        {
            _statsApi = new StatsApi(new SqliteAdapter(), true);

            //_workers = new Thread[maxThreads];
            _workers = new ApiWorkerThread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(this.HandleRequests);
        }

        public void Start(string prefix)
        {
            lock (_listener)
            {
                _listener.Prefixes.Clear();
                _listener.Prefixes.Add(prefix);
                _listener.Start();
                _listenerThread.Start();

                for (int i = 0; i < _workers.Length; i++)
                {
                    //_workers[i] = new Thread(this.Worker);
                    _workers[i] = new ApiWorkerThread(new StatsApi(new SqliteAdapter(), false), _queue, _ready, _stop);
                    _workers[i].Start();
                }
            }
        }

        public void Stop()
        {
            lock (_listener)
            {
                _stop.Set();
                _listenerThread.Join();
                //foreach (Thread worker in _workers)
                foreach (var worker in _workers)
                {
                    worker.Join();
                }

                _listener.Stop();
            }
        }

        public void Dispose()
        {
            this.Stop();
        }

        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (_queue)
                {
                    _queue.Enqueue(_listener.EndGetContext(ar));
                    _ready.Set();
                }
            }
            catch
            {
                return;
            }
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
            {
                var context = _listener.BeginGetContext(this.ContextReady, null);

                if (0 == WaitHandle.WaitAny(new[] {_stop, context.AsyncWaitHandle}))
                    return;
            }
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
                    _statsApi.HandleIncorrect(context);
                }

                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    _statsApi.HandleIncorrect(context);
                    Extras.WriteColoredLine(
                        $"Source: {error.Source}\nException: {error.Message}\nStack Trace: {error.StackTrace}",
                        ConsoleColor.Red);
                }
            }
        }

        //public event Action<HttpListenerContext> ProcessRequest;

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
                        _statsApi.GetServersInfo(listenerContext);
                    // If method is not get
                    else _statsApi.HandleIncorrect(listenerContext);
                }
                else
                {
                    switch (parts[3])
                    {
                        case "info":
                            // /servers/<endpoint>/info PUT, GET
                            if (request.HttpMethod == HttpMethod.Get.Method)
                                _statsApi.GetServerInfo(listenerContext);
                            else if (request.HttpMethod == HttpMethod.Put.Method)
                                _statsApi.PutServerInfo(listenerContext);
                            else _statsApi.HandleIncorrect(listenerContext);
                            break;
                        case "matches":
                            // /servers/<endpoint>/matches/<timestamp> PUT, GET
                            if (request.HttpMethod == HttpMethod.Get.Method)
                                _statsApi.GetServerMatch(listenerContext);
                            else if (request.HttpMethod == HttpMethod.Put.Method)
                                _statsApi.PutServerMatch(listenerContext);
                            else _statsApi.HandleIncorrect(listenerContext);
                            break;
                        case "stats":
                            // /servers/<endpoint>/stats GET
                            _statsApi.GetServerStats(listenerContext);
                            break;
                        default:
                            _statsApi.HandleIncorrect(listenerContext);
                            break;
                    }
                }
            }
            else if (parts[1] == "reports" && request.HttpMethod == HttpMethod.Get.Method)
            {
                switch (parts[2])
                {
                    case "recent-matches":
                        _statsApi.GetRecentMatchesReport(listenerContext);
                        break;
                    case "best-players":
                        _statsApi.GetBestPlayersReport(listenerContext);
                        break;
                    case "popular-servers":
                        _statsApi.GetPopularServersReport(listenerContext);
                        break;
                    default:
                        _statsApi.HandleIncorrect(listenerContext);
                        break;
                }
            }
            else if (parts[1] == "players" && parts[3] == "stats" && request.HttpMethod == HttpMethod.Get.Method)
            {
                _statsApi.GetPlayerStats(listenerContext);
            }
            else
            {
                _statsApi.HandleIncorrect(listenerContext);
            }
        }
    }
}