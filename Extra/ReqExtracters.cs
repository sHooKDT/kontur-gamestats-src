using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Kontur.GameStats.Server
{
    public class ReqExtracters
    {
        public static string ExtractEndpoint(HttpListenerRequest req)
        {
            const string pattern = "\\/servers\\/[\\w\\d\\.]+-\\d+\\/.*";

            if (Regex.IsMatch(req.RawUrl, pattern)) return req.RawUrl.Split('/')[2];
            throw new ArgumentException("Incorrect url");
        }

        public static string ExtractName(HttpListenerRequest req)
        {
            const string pattern = "\\/players\\/[\\w\\d%]+\\/stats";

            if (Regex.IsMatch(req.RawUrl, pattern)) return req.RawUrl.Split('/')[2];
            throw new ArgumentException("Incorrect url");
        }

        public static DateTime ExtractTimestamp(HttpListenerRequest req)
        {
            const string pattern =
                "\\/servers\\/[\\w\\d\\.-]+\\/matches\\/\\d{1,4}-\\d{2}-\\d{1,2}T\\d{2}:\\d{2}:\\d{2}Z\\/?";

            if (Regex.IsMatch(req.RawUrl, pattern)) return DateTimeOffset.Parse(req.RawUrl.Split('/')[4]).UtcDateTime;
            throw new ArgumentException("Incorrect url");
        }

        public static int ExtractCount(HttpListenerRequest req)
        {
            const string pattern = "\\/reports\\/[\\w-]+\\/?\\/?(\\d+)?$";

            if (!Regex.IsMatch(req.RawUrl, pattern)) throw new ArgumentException("Incorrect Url");

            string[] spl = req.RawUrl.Split('/');
            // If count isn't set, default value = 5
            if (spl.Length < 4 || string.IsNullOrEmpty(spl[3])) return 5;

            int count = int.Parse(spl[3]);

            // 50, no more
            if (count >= 50) return 50;
            // 0, no less
            if (count <= 0) return 0;

            return count;
        }
    }
}