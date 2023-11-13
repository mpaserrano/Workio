using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Models.Admin.Logs;
using Workio.Models.Events;
using Workio.Services.Admin.Events;
using Workio.Services.Interfaces;
using Workio.Services.Events;
using Workio.Services.ReportServices;
using Microsoft.Extensions.Logging;

namespace Workio.Services.Admin.Log
{
    /// <summary>
    /// Representa a implementação da interface IAdminEventService com a lógica para obter os dados relevantes a eventos
    /// na base de dados.
    /// </summary>
    public class AdminEventService : IAdminEventService
    {
        private ApplicationDbContext _context;
        private UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;
        private IUserService _userService;
        private IEventsService _eventsService;
        private List<string> _authorizedRoles = new List<string>() { "Admin", "Mod" };

        public AdminEventService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IUserService userService, IEventsService eventsService)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _eventsService = eventsService;
        }


        /// <summary>
        /// Obtem todas os eventos.
        /// </summary>
        /// <returns>Collection de eventos</returns>
        public async Task<ICollection<Event>> GetEvents()
        {
            var list = await _context.Event.ToListAsync<Event>();
            if (list == null)
                return new List<Event>();
            return list;
        }

        /// <summary>
        /// Bane um evento.
        /// </summary>
        /// <returns>sucesso se baniu um evento</returns>
        public async Task<bool> BanEvent(Guid eventId)
        {
            var e = await _eventsService.GetEvent(eventId);
            if(e == null) return false;
            if(e.IsBanned) return false;

            e.IsBanned = true;
            _context.Event.Update(e);

            return (await _context.SaveChangesAsync() > 0);
        }

        /// <summary>
        /// Remove o ban um evento.
        /// </summary>
        /// <returns>sucesso se removeu o ban um evento</returns>
        public async Task<bool> UnbanEvent(Guid eventId)
        {
            var e = await _eventsService.GetEvent(eventId);
            if (e == null) return false;
            if (!e.IsBanned) return false;

            e.IsBanned = false;
            _context.Event.Update(e);

            return (await _context.SaveChangesAsync() > 0);
        }

        /// <summary>
        /// Mete um evento como featured
        /// </summary>
        /// <param name="eventId">Id do evento</param>
        /// <returns>true se foi alterado com sucesso, false caso contrário</returns>
        public async Task<bool> MarkAsFeatured(Guid eventId)
        {
            if(eventId == Guid.Empty) return false;

            var e = await _eventsService.GetEvent(eventId);
            if (e == null) return false;
            
            if (e.IsBanned) return false;

            if(e.IsFeatured) return false;

            if(e.State == EventState.Finish) return false;

            var currentUser = await GetCurrentUser();
            if (currentUser == null) { return false; }

            if (!(await isAuthorized(currentUser))) return false;

            var success = 0;
            try
            {
                e.IsFeatured = true;
                _context.Event.Update(e);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success == 1;
        }

        /// <summary>
        /// Remove um evento de featured
        /// </summary>
        /// <param name="eventId">Id do evento</param>
        /// <returns>true se foi alterado com sucesso, false caso contrário</returns>
        public async Task<bool> RemoveFeatured(Guid eventId)
        {
            if (eventId == Guid.Empty) return false;

            var e = await _eventsService.GetEvent(eventId);
            if (e == null) return false;

            if (e.IsBanned) return false;

            if (!e.IsFeatured) return false;

            if (e.State == EventState.Finish) return false;

            var currentUser = await GetCurrentUser();
            if (currentUser == null) { return false; }

            if (!(await isAuthorized(currentUser))) return false;

            var success = 0;
            try
            {
                e.IsFeatured = false;
                _context.Event.Update(e);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success == 1;
        }

        private async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            return await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        private async Task<bool> isAuthorized(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (roles == null) { return false; }

            bool containsAny = roles.Any(s => _authorizedRoles.Contains(s));

            return containsAny;
        }
    }
}
