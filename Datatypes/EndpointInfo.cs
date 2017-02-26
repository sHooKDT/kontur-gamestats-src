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
                string result = "";
                int N = gameModes.Length;
                for (int i = 0; i < N; i++)
                {
                    result += gameModes[i];
                    if (i != N - 1)
                    {
                        result += ",";
                    }
                }
                return result;
            }
        }

        public EndpointInfo(string endpoint, ServerInfo serverInfo)
        {
            this.endpoint = endpoint;
            info = serverInfo;
        }
    }
}
