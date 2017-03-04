using System;

namespace Kontur.GameStats.Server
{
    class Extras
    {
        public static DateTime UnixTimeToDateTime(double timestamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(timestamp).ToLocalTime();
            return dtDateTime;
        }

        public static void WriteColoredLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static double DateTimeToUnixTime(DateTime timestamp)
        {
            double unixTimestamp = timestamp.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return unixTimestamp;
        }
    }
}
