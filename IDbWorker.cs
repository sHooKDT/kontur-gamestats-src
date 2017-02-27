using System;
using Kontur.GameStats.Server.Datatypes;

namespace Kontur.GameStats.Server
{
    public interface IDbWorker
    {
        // Список всех серверов
        EndpointInfo[] GetServersInfo();

        // Информация о конкретном сервере
        EndpointInfo.ServerInfo GetServerInfo(string endpoint);

        // Кладет в базу данных новый сервер
        bool PutServerInfo(EndpointInfo server);

        // Получение матча по адресу сервера и времени
        MatchInfo GetServerMatch(string endpoint, DateTime timestamp);

        // Отправка данных о матче
        bool PutServerMatch(string endpoint, DateTime timestamp, MatchInfo match);

        // TODO: написать нужные типы данных
        /*
         * ServerStats
         * PlayerStats
         * RecentMatchesReport
         * BestPlayersReport
         * PopularServersReport
         */

        // Статистика сервера
        // ServerStats GetServerStats(string endpoint);

        // Статистика игрока
        // PlayerStats GetPlayerStats(string name);

        // Reports

        //RecentMatchesReport GetRecentMatchesReport(string endpoint, int count);

        //BestPlayersReport GetBestPlayersReport(string endpoint, int count);

        //PopularServersReport GetPopularServersReport(string endpoint, int count);
    }
}
