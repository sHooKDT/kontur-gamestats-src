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

            public ServerInfo(string name, string[] gamemodes)
            {
                this.name = name;
                gameModes = gamemodes;
            }

            public string GetGameModesString()
            {
                return string.Join(",", gameModes);
            }
        }

        public EndpointInfo(string endpoint, ServerInfo serverInfo)
        {
            this.endpoint = endpoint;
            info = serverInfo;
        }
    }
}
