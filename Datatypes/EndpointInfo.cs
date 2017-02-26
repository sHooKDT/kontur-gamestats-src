namespace Kontur.GameStats.Server
{
    internal class EndpointInfo
    {
        public string endpoint;
        public ServerInfo info;

        public class ServerInfo
        {
            public string name;
            public string[] gameModes;
        }
    }
}
