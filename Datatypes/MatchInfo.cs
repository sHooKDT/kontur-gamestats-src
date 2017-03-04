namespace Kontur.GameStats.Server
{
    public class MatchInfo
    {
        public string map;
        public string gameMode;
        public int fragLimit;
        public int timeLimit;
        public double timeElapsed;
        public ScoreboardItem[] scoreboard;

        public class ScoreboardItem
        {
            public string name;
            public int frags;
            public int kills;
            public int deaths;

            public override string ToString()
            {
                return $"\"{name}\",{frags},{kills},{deaths}";
            }
        }
    }
}
