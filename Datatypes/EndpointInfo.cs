namespace Kontur.GameStats.Server
{
    public class EndpointInfo
    {
        private string endpoint;
        private ServerInfo info;

        public class ServerInfo
        {
            private string name;
            private string[] gameModes;

            public ServerInfo(string name, string gamemodes)
            {
                this.name = name;
                gameModes = gamemodes.Split(',');
            }

            public string GetName()
            {
                return name;
            }

            public string[] GetGameModes()
            {
                return gameModes;
            }
        }

        public EndpointInfo(string endpoint, string name, string gamemodes)
        {
            this.endpoint = endpoint;
            info = new ServerInfo(name, gamemodes);
        }

        public string GetEndpoint()
        {
            return endpoint;
        }

        public string GetName()
        {
            return info.GetName();
        }

        public string GetGameModes()
        {
            string result = "";
            int N = info.GetGameModes().Length;
            for (int i = 0; i < N; i++)
            {
                result += info.GetGameModes()[i];
                if(i != N - 1)
                {
                    result += ",";
                }
            }
            return result;
        }
    }
}
