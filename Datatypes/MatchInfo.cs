namespace Kontur.GameStats.Server.Datatypes
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
        }

        public MatchInfo()
        {

        }

    }
}
