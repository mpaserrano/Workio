using Microsoft.EntityFrameworkCore;
using Workio.Models;
using Workio.Models.Events;

namespace Workio.Services.Search
{
    public interface ISearchService
    {
        /// <summary>
        /// Obtem todos os utilizadores com um determinado nome
        /// </summary>
        /// <param name="name">Parte do nome do utilizador</param>
        /// <returns>Lista de utilizadores com um determinado nome ou parte dele</returns>
        Task<List<User>> GetUsersByName(string name);
        /// <summary>
        /// Obtem todos os utilizadores que o nome contenha partes.
        /// </summary>
        /// <param name="name">Parte do nome do utilizador</param>
        /// <returns>Lista de utilizadores com um determinado nome ou parte dele</returns>
        public Task<List<User>> GetUsersByNameIgnoreAccentuatedCharacters(string searchName);
        /// <summary>
        /// Obtem todos os utilizadores com um determinado email
        /// </summary>
        /// <param name="email">Email do utilizador a procurar</param>
        /// <returns>Lista de utilizadores com determidado email</returns>
        Task<List<User>> GetUsersByEmail(string email);

        /// <summary>
        /// Obtem todas as equipas com um determinado nome
        /// </summary>
        /// <param name="name">Parte do nome da equipa</param>
        /// <returns>Lista de equipas com um determinado nome ou parte dele</returns>
        Task<List<Team>> GetTeamsByName(string name);
        /// <summary>
        /// Obtem todas as equipas com um determinado nome ignorando caracteres pontuados.
        /// </summary>
        /// <param name="name">Parte do nome da equipa</param>
        /// <returns>Lista de equipas com um determinado nome ou parte dele</returns>
        Task<List<Team>> GetTeamsByNameIgnoreAccentuatedCharacters(string teamName);

        /// <summary>
        /// Obtem todas os eventos com um determinado nome
        /// </summary>
        /// <param name="name">Parte do nome da evento</param>
        /// <returns>Lista de eventos com um determinado nome ou parte dele</returns>
        Task<List<Event>> GetEventsByName(string name);
        /// <summary>
        /// Obtem todas os eventos com um determinado nome ignoranto caracteres pontuados.
        /// </summary>
        /// <param name="name">Parte do nome da evento</param>
        /// <returns>Lista de eventos com um determinado nome ou parte dele</returns>
        Task<List<Event>> GetEventsByNameIgnoreAccentuatedCharacters(string eventName);
    }
}
