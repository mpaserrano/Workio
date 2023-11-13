using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Models.Admin.Logs;
using Workio.Models.Events;
using Workio.Services.Admin.Teams;
using Workio.Services.Interfaces;
using Workio.Services.ReportServices;
using Workio.Services.Teams;

namespace Workio.Services.Admin.Log
{
    /// <summary>
    /// Representa a implementação da interface IAdminTeamService com a lógica para obter os dados relevantes a equipas
    /// na base de dados.
    /// </summary>
    public class AdminTeamService : IAdminTeamService
    {
        private ApplicationDbContext _context;
        private UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;
        private IUserService _userService;
        private ITeamsService _teamsService;

        public AdminTeamService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IUserService userService, ITeamsService teamsService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _teamsService = teamsService;
        }


        /// <summary>
        /// Obtem todas as equipas.
        /// </summary>
        /// <returns>Collection de equipas</returns>
        public async Task<ICollection<Team>> GetTeams()
        {
            return await _context.Team
                .Include(t => t.Members)
                .Include(t => t.Skills)
                .Include(t => t.Positions)
                .Include(t => t.PendingList)
                .ToListAsync();
        }

        /// <summary>
        /// Bane uma equipa.
        /// </summary>
        /// <returns>sucesso se baniu uma equipa</returns>
        public async Task<bool> BanTeam(Guid teamId)
        {
            var team = await _teamsService.GetTeamById(teamId);
            if (team == null) return false;
            if (team.IsBanned) return false;

            team.IsBanned = true;
            _context.Team.Update(team);

            return (await _context.SaveChangesAsync() > 0);
        }

        /// <summary>
        /// remove o ban de uma equipa.
        /// </summary>
        /// <returns>sucesso se tirou baniu uma equipa</returns>
        public async Task<bool> UnbanTeam(Guid teamId)
        {
            var team = await _teamsService.GetTeamById(teamId);
            if (team == null) return false;
            if (!team.IsBanned) return false;

            team.IsBanned = false;
            _context.Team.Update(team);

            return (await _context.SaveChangesAsync() > 0);
        }


    }
}
