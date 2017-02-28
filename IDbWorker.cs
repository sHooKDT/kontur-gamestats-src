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
