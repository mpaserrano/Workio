using System;
using Workio.Models;
using Workio.Models.Events;

namespace Workio.Services.Matchmaking
{
    /// <summary>
    /// Fornece a interface para serviços de consulta à base de dados relativamente à comparação de informação para obter recomendação para os utilizadores.
    /// </summary>
    public interface IMatchmakingService
    {

        /// <summary>
        /// Obtem todas as equipas e ordena-as de acordo com o algoritmo de matchmaking.
        /// O algoritmo utiliza 5 fatores:
        /// -Tags totais do utilizador (TU)
        /// -Tags totais da equipa (TE)
        /// -Tags compativeis (tem o mesmo nome) entre o utililizador e a equipa (TC)
        /// -Rating do utilizador (RU)
        /// -Rating médio da equipa (media da rating de todos os utilizadores da equipa (RE)
        /// Formula:
        /// ((1/TE* TC) + (1/TU* TC)) - (MAX(RE, RU) - MIN(RE, RU))/10
        /// Esta formula devolve um score entre -0.5 e 2
        /// -0.5 é a pior match e 2 é a melhor match
        /// </summary>
        /// <returns>Lista de equipas ordenada</returns>
        public Task<List<Team>> GetRecommendedTeams();
        /// <summary>
        /// Metodo para calcular o top 5 de eventos para o utilizador
        /// </summary>
        /// <returns>Devolve uma lista com os 5 melhores eventos para o utilizador</returns>
        public Task<List<Event>> GetRecommendedEvents();

        /// <summary>
        /// Recebe informações da localização do utilizador e área a abranger de pesquisa e
        /// retorna uma lista de eventos dos eventos presenciaís que se encontrem nessa área.
        /// </summary>
        /// <param name="latitute">Latitude da posição do utilizador.</param>
        /// <param name="longitude">Logintude da posição do utilizador.</param>
        /// <param name="distance">Área de procura por eventos.</param>
        /// <returns>Lista de eventos que estão numa área circular relativamente à posição do utilizador.</returns>
        public Task<List<Event>> GetEventsNear(double latitute, double longitude, double distance);

        /// <summary>
        /// Recebe informações sobre uma localização e obtem eventos próximos a essa localização até uma
        /// determinada distância.
        /// </summary>
        /// <param name="latitute">Latitude da posição do utilizador.</param>
        /// <param name="longitude">Logintude da posição do utilizador.</param>
        /// <param name="distance">Área de procura por eventos.</param>
        /// <returns>Lista de eventos que estão numa área circular relativamente à posição do utilizador.</returns>
        public Task<IEnumerable<Object>> GetEventsNearWithDistances(double latitute, double longitude, double distance);

    }
}
