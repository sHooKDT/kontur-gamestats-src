using System;

namespace Kontur.GameStats.Server
{
    class PlayerStats
    {
        public int totalMatchesPlayed;
        public int totalMatchesWon;
        public string favoriteServer;
        public int uniqueServers;
        public string favoriteGameMode;
        public float averageScoreboardPercent;
        public int maximumMatchesPerDay;
        public float averageMatchesPerDay;
        public DateTime lastMatchPlayed;
        public float killToDeathRatio;
    }
}
