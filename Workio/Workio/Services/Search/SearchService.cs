using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Xml.Linq;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;

namespace Workio.Services.Search
{
    public class SearchService : ISearchService
    {

        /// <summary>
        /// Contexto da Base de Dados
        /// </summary>
        private ApplicationDbContext _context;
        private IHttpContextAccessor _httpContextAccessor;

        public SearchService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Obtem todos os utilizadores com um determinado email
        /// </summary>
        /// <param name="email">Email do utilizador a procurar</param>
        /// <returns>Lista de utilizadores com determidado email</returns>
        public async Task<List<User>> GetUsersByEmail(string email)
        {
            return await _context.Users
                .Include(x => x.BlockedUsers)
                .Include(x => x.TeamsRequests)
                .Include(x => x.Teams)
                .Where(u => u.Email == email).ToListAsync();
        }

        /// <summary>
        /// Obtem todos os utilizadores com um determinado nome
        /// </summary>
        /// <param name="name">Parte do nome do utilizador</param>
        /// <returns>Lista de utilizadores com um determinado nome ou parte dele</returns>
        public async Task<List<User>> GetUsersByName(string name)
        {
            return await _context.Users
                .Include(x => x.BlockedUsers)
                .Include(x => x.TeamsRequests)
                .Include(x => x.Teams)
                .Where(u => u.Name.Contains(name)).ToListAsync();
        }

        /// <summary>
        /// Obtem todos os utilizadores com um determinado email
        /// </summary>
        /// <param name="email">Email do utilizador a procurar</param>
        /// <returns>Lista de utilizadores com determidado email</returns>
        public async Task<List<User>> GetUsersByNameIgnoreAccentuatedCharacters(string searchName)
        {
            var query = _context.Users.FromSqlRaw("SELECT * FROM dbo.AspNetUsers WHERE Name like '%' + @searchName + '%' COLLATE Latin1_general_CI_AI", new SqlParameter("@searchName", searchName));

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                return await query.Include(b => b.BlockedUsers).Where(u => (!u.BlockedUsers.Any(b => b.BlockedUserId == currentUserId)) && u.LockoutEnd == null).ToListAsync();
            }
            else
            {
                return await query.Where(u => u.LockoutEnd == null).ToListAsync();
            }
        }

        /// <summary>
        /// Obtem todas as equipas com um determinado nome
        /// </summary>
        /// <param name="name">Parte do nome da equipa</param>
        /// <returns>Lista de equipas com um determinado nome ou parte dele</returns>
        public async Task<List<Team>> GetTeamsByName(string name)
        {
            return await _context.Team.Where(t => t.TeamName.Contains(name) && t.IsBanned == false).ToListAsync();
        }

        /// <summary>
        /// Obtem todas as equipas com um determinado nome ignorando caracteres pontuados.
        /// </summary>
        /// <param name="name">Parte do nome da equipa</param>
        /// <returns>Lista de equipas com um determinado nome ou parte dele</returns>
        public async Task<List<Team>> GetTeamsByNameIgnoreAccentuatedCharacters(string teamName)
        {
            var query = _context.Team.FromSqlRaw("SELECT * FROM dbo.Team WHERE TeamName like '%' + @teamName + '%' COLLATE Latin1_general_CI_AI", new SqlParameter("@teamName", teamName));

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                return await query.Where(t => t.IsBanned != true || t.OwnerId.ToString() == currentUserId).ToListAsync();
            }
            else
            {
                return await query.Where(t => t.IsBanned != true).ToListAsync();
            }
        }

        /// <summary>
        /// Obtem todas os eventos com um determinado nome
        /// </summary>
        /// <param name="name">Parte do nome da evento</param>
        /// <returns>Lista de eventos com um determinado nome ou parte dele</returns>
        public async Task<List<Event>> GetEventsByName(string name)
        {
            return await _context.Event.Where(e => e.Title.Contains(name) && e.IsBanned == false).ToListAsync();
        }

        /// <summary>
        /// Obtem todas os eventos com um determinado nome ignoranto caracteres pontuados.
        /// </summary>
        /// <param name="name">Parte do nome da evento</param>
        /// <returns>Lista de eventos com um determinado nome ou parte dele</returns>
        public async Task<List<Event>> GetEventsByNameIgnoreAccentuatedCharacters(string eventName)
        {
            var query = _context.Event.FromSqlRaw("SELECT * FROM dbo.Event WHERE Title like '%' + @eventName + '%' COLLATE Latin1_general_CI_AI", new SqlParameter("@eventName", eventName));

            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                string currentUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                return await query.Where(e => e.IsBanned != true || e.UserId == currentUserId).ToListAsync();
            }
            else
            {
                return await query.Where(e => e.IsBanned != true).ToListAsync();
            }
        }
    }
}
