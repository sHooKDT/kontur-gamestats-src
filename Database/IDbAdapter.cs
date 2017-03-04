using System;
using System.Collections.Generic;

namespace Kontur.GameStats.Server
{
    public interface IDbAdapter
    {
        // Список всех серверов
        IEnumerable<EndpointInfo> GetServersInfo();

        // Информация о конкретном сервере
        EndpointInfo.ServerInfo GetServerInfo(string endpoint);

        // Кладет в базу данных новый сервер
        bool PutServerInfo(EndpointInfo server);

        // Получение матча по адресу сервера и времени
        MatchInfo GetServerMatch(string endpoint, DateTime timestamp);

        // Отправка данных о матче
        bool PutServerMatch(string endpoint, DateTime timestamp, MatchInfo match);

        //Статистика сервера
        string MakeServerStats(string endpoint);

        // Статистика игрока
        string MakePlayerStats(string name);

        // Reports

        string MakeRecentMatchesReport(int count);

        string MakeBestPlayersReport(int count);

        string MakePopularServersReport(int count);
    }
}
