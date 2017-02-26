namespace Kontur.GameStats.Server
{
    public class EndpointInfo
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
