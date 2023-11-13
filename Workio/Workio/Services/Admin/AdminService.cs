using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Services.Interfaces;

namespace Workio.Services.Admin
{
    public class AdminService : IAdminService
    {
        /// <summary>
        /// Representa a implementação da interface IAdminService com a lógica para guardar os dados referentes referentes a pedidos de entidades registadas
        /// </summary>
        private readonly ApplicationDbContext _context;
        private IUserService _userService;
        private readonly UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;
        private List<string> _authorizedRoles = new List<string>() { "Admin", "Mod"};

        /// <summary>
        /// Constructor da classe.
        /// </summary>
        /// <param name="_context">Contexto da base de dados</param>
        /// <param name="_userService">Variavel para aceder ao serviço de utilizadores</param>
        /// <param name="_userManager">Variavel para aceder ao UserManager</param>
        /// <param name="_httpcontextAcessor">Variavel para aceder ao IHttpContextAcessor</param>
        public AdminService(ApplicationDbContext context, IUserService userService, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Obtem uma lista de pedidos de entidade registadas
        /// </summary>
        /// <returns>Lista de pedidos de entidades registadas</returns>

        public async Task<List<RequestEntityStatus>> GetRequestsAsync()
        {

            return await _context.RequestEntityStatus.ToListAsync();
        }
        /// <summary>
        /// Verifica se é possível aprovar um pedido para ser entidade registada
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>True: Se bem sucedido False: Se Falhou</returns>
        public async Task<bool> ApproveRequest(Guid id)
        {
            var request = await GetRequestById(id);
            var currentUser = await GetCurrentUser();
            if (request.RequestState == RequestState.Approved || request.RequestState == RequestState.Rejected)
            {
                return false;
            }
            var user = await _userService.GetUser(request.UserId.Value);
            var isEntity = await _userManager.IsInRoleAsync(user, "Entity");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isMod = await _userManager.IsInRoleAsync(user, "Mod");
            var isCurrentUserAllowed = (await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "Mod"));
            if (isEntity || isAdmin || isMod)
            {
                return false;
            }
            if (!isCurrentUserAllowed)
            {
                return false;
            }
            await _userManager.AddToRoleAsync(user, "Entity");
            request.RequestState = RequestState.Approved;
            await _context.SaveChangesAsync();
            return true;

        }
        /// <summary>
        /// Verifica se é possível rejeitar um pedido para ser entidade registada
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>True: Se bem sucedido False: Se Falhou</returns>

        public async Task<bool> RejectRequest(Guid id)
        {
            var request = await GetRequestById(id);
            var currentUser = await GetCurrentUser();
            var user = await _userService.GetUser(request.UserId.Value);
            var isEntity = await _userManager.IsInRoleAsync(user, "Entity");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isMod = await _userManager.IsInRoleAsync(user, "Mod");
            var isCurrentUserAllowed = (await _userManager.IsInRoleAsync(currentUser, "Admin") || await _userManager.IsInRoleAsync(currentUser, "Mod"));
            if (isEntity || isAdmin || isMod)
            {
                return false;
            }
            if (request.RequestState == RequestState.Approved)
            {
                return false;
            }
            if (!isCurrentUserAllowed)
            {
                return false;
            }
            request.RequestState = RequestState.Rejected;
            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Obtem um pedido de entidade registada pelo seu id
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>RequestEntityStatus - pedido com o id correspondente</returns>
        public async Task<RequestEntityStatus> GetRequestById(Guid id)
        {
            return await _context.RequestEntityStatus.Where(r => r.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtem uma lista de de todos os pedidos de entidade registada
        /// </summary>
        /// <returns>Lista com os pedidos</returns>
        public async Task<List<RequestEntityStatus>> GetRequests()
        {
            return await _context.RequestEntityStatus.ToListAsync();
        }

        /// <summary>
        /// Suspende um utilizador durante x dias
        /// </summary>
        /// <param name="userId">Id do utilizador a suspender</param>
        /// <param name="duration">Duração da suspensão em dias (default são 5 dias)</param>
        /// <returns>True se o utilizador foi suspenso com sucesso, false caso contrário</returns>
        public async Task<bool> SuspendUser(Guid userId, int duration = 5)
        {
            var currentUser = await GetCurrentUser();
            if(currentUser == null || userId.ToString() == currentUser.Id) { return false; }

            if (!(await isAuthorized(currentUser))) return false;

            var user = await _userService.GetUser(userId);

            if(user == null) return false;

            if ((await _userManager.IsInRoleAsync(user, "Admin")) || (await _userManager.IsInRoleAsync(user, "Mod"))) return false;

            // Suspend the user
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(duration));

            return result.Succeeded;
        }

        /// <summary>
        /// Remove a suspensão a um utilizador 
        /// </summary>
        /// <param name="userId">Id do utilizador a suspender</param>
        /// <returns>True se o utilizador foi reintegrado com sucesso, false caso contrário</returns>
        public async Task<bool> UnsuspendUser(Guid userId)
        {
            var currentUser = await GetCurrentUser();
            if (currentUser == null) { return false; }

            if (!(await isAuthorized(currentUser))) return false;

            var user = await _userService.GetUser(userId);

            if (user == null) return false;

            // Suspend the user
            var result = await _userManager.SetLockoutEndDateAsync(user, null);

            return result.Succeeded;
        }

        /// <summary>
        /// Obtem uma lista de reports que foram feitos contra o utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Lista de denúncias</returns>
        public async Task<List<ReportUser>> GetUserReports(Guid id)
        {
            var currentUser = await GetCurrentUser();
            if (currentUser == null) { return new List<ReportUser>(); }

            if (!(await isAuthorized(currentUser))) return new List<ReportUser>();

            var reports = await _context.ReportUser
                    .Include(x => x.ReportReason)
                .Where(u => u.ReportedId == id.ToString()).ToListAsync();

            if(reports == null) return new List<ReportUser>();

            return reports;
        }

        /// <summary>
        /// Obtem uma lista de reports que foram feitos contra o equipas que o utilizador participa
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Lista de denúncias</returns>
        public async Task<List<ReportTeam>> GetUserTeamsReports(Guid id)
        {
            var currentUser = await GetCurrentUser();
            if (currentUser == null) { return new List<ReportTeam>(); }

            if (!(await isAuthorized(currentUser))) return new List<ReportTeam>();

            var userTeams = await _context.Team.Include(t => t.Members).Where(t => t.OwnerId == id || t.Members.Any(m => m.Id == id.ToString())).ToListAsync();

            var reports = await _context.ReportTeams
                    .Include(x => x.ReportReason)
                    .Include(x => x.ReportedTeam)
                .Where(u => userTeams.Contains(u.ReportedTeam)).ToListAsync();

            if (reports == null) return new List<ReportTeam>();

            return reports;
        }

        /// <summary>
        /// Obtem uma lista de reports que foram feitos contra os eventos que o utilizador criou
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Lista de denúncias</returns>
        public async Task<List<ReportEvent>> GetUserEventsReports(Guid id)
        {
            var currentUser = await GetCurrentUser();
            if (currentUser == null) { return new List<ReportEvent>(); }

            if (!(await isAuthorized(currentUser))) return new List<ReportEvent>();

            var userEvents = await _context.Event.Where(t => t.UserId == id.ToString()).ToListAsync();

            var reports = await _context.ReportEvents
                    .Include(x => x.ReportReason)
                    .Include(x => x.ReportedEvent)
                .Where(u => userEvents.Contains(u.ReportedEvent)).ToListAsync();

            if (reports == null) return new List<ReportEvent>();

            return reports;
        }

        private async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            return await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private async Task<bool> isAuthorized(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if(roles == null) { return false; }

            bool containsAny = roles.Any(s => _authorizedRoles.Contains(s));

            return containsAny;
        }
    }
}