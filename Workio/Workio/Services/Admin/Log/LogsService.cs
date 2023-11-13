using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Models.Admin.Logs;
using Workio.Models.Events;
using Workio.Services.Interfaces;

namespace Workio.Services.Admin.Log
{
    /// <summary>
    /// Representa a implementação da interface ILogsService com a lógica para guardar os dados referentes a logs
    /// na base de dados.
    /// </summary>
    public class LogsService : ILogsService
    {
        private ApplicationDbContext _context;
        private UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;

        public LogsService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CreateAdminActionLog(string description, AdministrationActionLogType? actionType)
        {
            User user = await GetCurrentUser();
            AdministrationActionLog administrationActionLog = new AdministrationActionLog();

            administrationActionLog.LogId = Guid.NewGuid();
            administrationActionLog.AuthorId = user.Id;

            administrationActionLog.ActionDescription = description;
            administrationActionLog.ActionLogType = actionType;

            var success = 0;

            try
            {
                _context.Add(administrationActionLog);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        public async Task<bool> CreateEventActionLog(string description, string changedEventId, EventActionLogType? actionType)
        {
            User user = await GetCurrentUser();
            EventActionLog eventActionLog = new EventActionLog();

            eventActionLog.LogId = Guid.NewGuid();
            eventActionLog.AuthorId = user.Id;

            eventActionLog.ActionDescription = description;
            eventActionLog.ChangedEventId = new Guid(changedEventId);
            eventActionLog.ActionLogType = actionType;

            var success = 0;

            try
            {
                _context.Add(eventActionLog);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        public async Task<bool> CreateSystemLog(string description)
        {
            SystemLog systemLog = new SystemLog();
            systemLog.LogId = Guid.NewGuid();
            systemLog.Description = description;

            var success = 0;

            try
            {
                _context.Add(systemLog);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        public async Task<bool> CreateTeamActionLog(string description, string changedTeamId, TeamActionLogType? actionType)
        {
            User user = await GetCurrentUser();
            TeamActionLog teamActionLog = new TeamActionLog();

            teamActionLog.LogId = Guid.NewGuid();
            teamActionLog.AuthorId = user.Id;

            teamActionLog.ActionDescription = description;
            teamActionLog.ChangedTeamId = new Guid(changedTeamId);
            teamActionLog.ActionLogType = actionType;

            var success = 0;

            try
            {
                _context.Add(teamActionLog);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        public async Task<bool> CreateUserActionLog(string description, string changedUserId, UserActionLogType? actionType)
        {
            User user = await GetCurrentUser();
            UserActionLog userActionLog = new UserActionLog();
            userActionLog.LogId = Guid.NewGuid();
            userActionLog.AuthorId = user.Id;
            userActionLog.ActionDescription = description;
            userActionLog.ChangedUserId = changedUserId;
            userActionLog.ActionLogType = actionType;

            var success = 0;

            try
            {
                _context.Add(userActionLog);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        public async Task<IEnumerable<object>> GetAdminActionLogDataFiltered(string search)
        {
            var returnData = _context.AdministrationActionLogs.OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    Id = x.LogId,
                    AuthorId = x.AuthorId,
                    AuthorName = x.AuthorUser.Name,
                    AuthorProfilePic = x.AuthorUser.ProfilePicture,
                    Description = x.ActionDescription,
                    ActionType = x.ActionLogType.ToString(),
                    Timestamp = x.CreatedDate.ToString("yyyy/MM/dd HH:mm")
                });

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                returnData = returnData.Where(x => (x.AuthorName.ToLower().Contains(search)));
            }

            return await returnData.ToListAsync();
        }

        public async Task<IEnumerable<object>> GetEventActionLogDataFiltered(string search)
        {
            var returnData = _context.EventActionLogs.OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    Id = x.LogId,
                    AuthorId = x.AuthorId,
                    AuthorName = x.AuthorUser.Name,
                    AuthorProfilePic = x.AuthorUser.ProfilePicture,
                    ChangedEventId = x.ChangedEventId,
                    ChangedEventName = x.ChangedEvent.Title,
                    ActionType = x.ActionLogType.ToString(),
                    Timestamp = x.CreatedDate.ToString("yyyy/MM/dd HH:mm")
                });

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                returnData = returnData.Where(x => (x.AuthorName.ToLower().Contains(search)) || (x.ChangedEventName.ToLower().Contains(search)));
            }

            return await returnData.ToListAsync();
        }

        public async Task<IEnumerable<object>> GetTeamActionLogDataFiltered(string search)
        {
            var returnData = _context.TeamActionLogs.OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    Id = x.LogId,
                    AuthorId = x.AuthorId,
                    AuthorName = x.AuthorUser.Name,
                    AuthorProfilePic = x.AuthorUser.ProfilePicture,
                    ChangedTeamId = x.ChangedTeamId,
                    ChangedTeamName = x.ChangedTeam.TeamName,
                    ActionType = x.ActionLogType.ToString(),
                    Timestamp = x.CreatedDate.ToString("yyyy/MM/dd HH:mm")
                });

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                returnData = returnData.Where(x => (x.AuthorName.ToLower().Contains(search)) || (x.ChangedTeamName.ToLower().Contains(search)));
            }

            return await returnData.ToListAsync();
        }

        public async Task<IEnumerable<Object>> GetUserActionDataFiltered(string search)
        {
            var returnData = _context.UserActionLog.OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    Id = x.LogId,
                    AuthorId = x.AuthorId,
                    AuthorName = x.AuthorUser.Name,
                    AuthorProfilePic = x.AuthorUser.ProfilePicture,
                    ChangedUserId = x.ChangedUserId,
                    ChangedUserName = x.ChangedUser.Name,
                    ChangedUserProfilePic = x.ChangedUser.ProfilePicture,
                    ActionType = x.ActionLogType.ToString(),
                    Description = x.ActionDescription,
                    Timestamp = x.CreatedDate.ToString("yyyy/MM/dd HH:mm")
                });

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                returnData = returnData.Where(x => (x.AuthorName.ToLower().Contains(search)) || (x.ChangedUserName.ToLower().Contains(search)));
            }

            return await returnData.ToListAsync();
        }

        public async Task<List<UserActionLog>> GetUserActionLogData()
        {
            return await _context.UserActionLog
                .Include(x => x.ChangedUser)
                .OrderBy(x => x.LogId).ToListAsync();
        }

        public async Task<UserActionLog> GetUserActionLogByLogId(string userActionLogId)
        {
            return await _context.UserActionLog
                .Include(l => l.AuthorUser)
                .Include(l => l.ChangedUser)
                .FirstOrDefaultAsync(ual => ual.LogId.ToString() == userActionLogId);
        }

        /// <summary>
        /// Obtem o utilizador logged in.
        /// </summary>
        /// <returns>Utilizador Loggedin</returns>
        private async Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            return await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public async Task<TeamActionLog> GetTeamActionLogByLogId(string teamActionLogId)
        {
            return await _context.TeamActionLogs
                .Include(t => t.ChangedTeam)
                .FirstOrDefaultAsync(tal => tal.LogId.ToString() == teamActionLogId);
        }

        public async Task<EventActionLog> GetEventActionLogByLogId(string eventActionLogId)
        {
            return await _context.EventActionLogs
                .Include(e => e.ChangedEvent)
                .FirstOrDefaultAsync(eal => eal.LogId.ToString() == eventActionLogId);
        }

        public async Task<AdministrationActionLog> GetAdministrationActionLogByLogId(string administrationActionLogId)
        {
            return await _context.AdministrationActionLogs
                .FirstOrDefaultAsync(aal => aal.LogId.ToString() == administrationActionLogId);
        }
    }
}
