using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NToastNotify;
using Org.BouncyCastle.Utilities.IO;
using Workio.Managers.Notifications;
using Workio.Models;
using Workio.Services;
using Workio.Services.Admin;
using Workio.Services.Admin.Log;
using Workio.Services.Email.Interfaces;
using Workio.Services.Interfaces;
using Workio.Services.ReportServices;
using Workio.Services.Teams;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador que dá handle dos pedidos da administração para gerir os utilizadores
    /// </summary>
    [Authorize(Roles = "Admin, Mod")]
    public class AdminController : Controller
    {

        private readonly IReportReasonService _reportReasonService;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly ILogsService _logsService;
        private readonly IToastNotification _toastNotification;
        private readonly IEmailService _emailService;
        private readonly ITeamsService _teamsService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly CommonLocalizationService _localizationService;
        private readonly INotificationManager _notificationManager;

        private const string _MaxRoleName = "Admin";

        public AdminController(IReportReasonService reportReasonService, IUserService userService, IAdminService adminService, 
            ILogsService logsService, IToastNotification toastNotification, IEmailService emailService, ITeamsService teamsService,
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager, CommonLocalizationService localizationService,
            INotificationManager notificationManager)
        {
            _reportReasonService = reportReasonService;
            _userService = userService;
            _adminService = adminService;
            _logsService = logsService;
            _toastNotification = toastNotification;
            _emailService = emailService;
            _teamsService = teamsService;
            _userManager = userManager;
            _roleManager = roleManager;
            _localizationService = localizationService;
            _notificationManager = notificationManager;
        }

        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> Index()
        {
            //ViewBag.TeamReports = await _reportReasonService.GetTeamReports();
            //ViewBag.UserReports = await _reportReasonService.GetUserReports();
            //return View();
            return RedirectToAction("Users");
        }

        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> Users()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> User(Guid id)
        {
            if(id == Guid.Empty)
            {
                return NotFound();
            }
            
            var user = await _userService.GetUser(id);
            
            if(user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            if(roles.Contains("Mod") || roles.Contains("Admin") || roles.Contains("Entity"))
            {
                ViewBag.showEventsReports = true;
            }
            else
            {
                ViewBag.showEventsReports = false;
            }


            return View(user);
        }

        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> BanUser(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();

            var success = await _adminService.SuspendUser(id);

            if (success)
            {
                var log = await _logsService.CreateUserActionLog(logModel.Description, id.ToString(), Models.Admin.Logs.UserActionLogType.Banned);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("UserSuspendedSuccess"));
                await SendUserBanEmail(id);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("UserSuspendsError"));
            }

            return RedirectToAction(nameof(Users));
        }

        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> UnbanUser(Guid id, LogViewModel logModel)
        {
            if (id == null) return NotFound();

            var success = await _adminService.UnsuspendUser(id);

            if (success)
            {
                var log = await _logsService.CreateUserActionLog(logModel.Description, id.ToString(), Models.Admin.Logs.UserActionLogType.Unbanned);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("UserUnsuspendedSuccess"));
                await SendUserUnbanEmail(id);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("UserUnsuspendsError"));
            }

            return RedirectToAction(nameof(Users));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole(Guid id, Guid roleId, LogViewModel logModel)
        {
            if (Guid.Empty == roleId || Guid.Empty == id) return NotFound();
            if (logModel == null || string.IsNullOrEmpty(logModel.Description))
            {
                return BadRequest();
            }

            var role = await _roleManager.Roles.Where(x => x.Id == roleId.ToString()).FirstOrDefaultAsync();

            if (role == null) { BadRequest(); }

            var user = await _userService.GetUser(id);

            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Contains(role.Name))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("UserAlreadyHasRole"));
                //return BadRequest();
                return RedirectToAction(nameof(User), new { id = id });
            }

            if(role.Name == _MaxRoleName)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("CantGiveAdminRole"));
                return RedirectToAction(nameof(User), new { id = id });
            }

            if (userRoles.Contains(_MaxRoleName))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("CanChangeAdminRoles"));
                //return BadRequest();
                return RedirectToAction(nameof(User), new { id = id });
            }

            var suc = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (suc.Succeeded)
            {
                var result = await _userManager.AddToRoleAsync(user, role.Name);

                if (result.Succeeded)
                {
                    var log = await _logsService.CreateUserActionLog(logModel.Description + "/n User now has permission: " + role.Name, id.ToString(), Models.Admin.Logs.UserActionLogType.GivedRole);
                    _toastNotification.AddSuccessToastMessage(_localizationService.Get("RoleChanged"));
                    return RedirectToAction(nameof(User), new { id = id });
                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizationService.Get("RoleChangedError"));
                    //return Problem();
                    return RedirectToAction(nameof(User), new { id = id });
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("RoleChangedError"));
                //return Problem();
                return RedirectToAction(nameof(User), new { id = id });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(Guid id, Guid roleId, LogViewModel logModel)
        {
            if(Guid.Empty == roleId || Guid.Empty == id) return NotFound();
            if (logModel == null || string.IsNullOrEmpty(logModel.Description))
            {
                return BadRequest();
            }
           
            var role = await _roleManager.Roles.Where(x => x.Id == roleId.ToString()).FirstOrDefaultAsync();

            if(role == null) { BadRequest(); }

            var user = await _userService.GetUser(id);

            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Contains(role.Name))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("UserAlreadyHasRole"));
                //return BadRequest();
                return RedirectToAction(nameof(User), new { id = id });
            }

            if (userRoles.Contains(_MaxRoleName))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("CanAddRolesToAdmin"));
                //return BadRequest();
                return RedirectToAction(nameof(User), new { id = id });
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if(result.Succeeded)
            {
                var log = await _logsService.CreateUserActionLog(logModel.Description + "/n Role added: " + role.Name, id.ToString(), Models.Admin.Logs.UserActionLogType.GivedRole);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("RoleAddedToUser"));
                return RedirectToAction(nameof(User), new { id = id});
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("RoleAddedToUser"));
                //return Problem();
                return RedirectToAction(nameof(User), new { id = id });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(Guid id, Guid roleId, LogViewModel logModel)
        {
            if (Guid.Empty == roleId || Guid.Empty == id) return NotFound();
            if (logModel == null || string.IsNullOrEmpty(logModel.Description))
            {
                return BadRequest();
            }

            var role = await _roleManager.Roles.Where(x => x.Id == roleId.ToString()).FirstOrDefaultAsync();

            if (role == null) { BadRequest(); }

            var user = await _userService.GetUser(id);

            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains(role.Name))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("UserDoesntHaveRole"));
                //return BadRequest();
                return RedirectToAction(nameof(User), new { id = id });
            }

            if (userRoles.Contains(_MaxRoleName))
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("CantRemoveRolesFromAdmin"));
                //return BadRequest();
                return RedirectToAction(nameof(User), new { id = id });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                var log = await _logsService.CreateUserActionLog(logModel.Description + "/n Role removed: " + role.Name, id.ToString(), Models.Admin.Logs.UserActionLogType.RemovedRole);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("RoleRemovedFromUser"));
                return RedirectToAction(nameof(User), new { id = id });
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("RoleAddedToUser"));
                //return Problem();
                return RedirectToAction(nameof(User), new { id = id });
            }
        }

        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetUsers(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var usersData = await _userService.GetUsersAsync();

            var result = usersData.Select(x => new { x.Id, x.Name, x.Email, x.ProfilePicture, lockoutend = x.LockoutEnd == null ?  _localizationService.Get("NotSuspended") : x.LockoutEnd.Humanize(), banned = x.LockoutEnd == null ? false : true});
            
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                result = result.Where(x => (x.Name.ToLower().Contains(searchValue))).ToList();
            }

            // Perform filtering and sorting operations using the provided parameters.

            // Calculate the total number of records that match the search criteria.
            int totalRecords = result.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = result.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = result.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();
            //var usersLogData = await _logsService.GetUserActionLogData();
            var userData = await _userService.GetUser(id);

            if (userData == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(userData);

            var roles = _roleManager.Roles.ToList();

            if (!userRoles.Contains(_MaxRoleName))
            {
                roles = roles.Where(r => r.Name != _MaxRoleName).ToList();
            }

            var result = roles.Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                UserHasRole = userRoles.Contains(x.Name),
            });

            // Construct the response object.
            var response = new
            {
                data = result
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Obtem uma lista em json das denúncias que foram feitas contra este utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>JSON com as denúncias contra um determinado utilizador</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetUserReports(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection, Guid id)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var rawReports = await _adminService.GetUserReports(id);

            var archivedReports = await _reportReasonService.GetArchiveUserReports();
            archivedReports = archivedReports.Where(x => x.ReportedId == id.ToString()).ToList();
            foreach (ReportUser r in archivedReports)
            {
                rawReports.Add(r);
            }

            var result = rawReports.Where(x => x.ReportedId == id.ToString()).Select(x => new
            {
                id = x.Id,
                reporterId = x.ReporterId,
                reporterName = _userService.GetUser(new Guid(x.ReporterId)).Result.Name,
                reporterProfilePicture = _userService.GetUser(new Guid(x.ReporterId)).Result.ProfilePicture,
                reportReason = x.ReportReason.Reason,
                reportStatus = x.ReportStatus,
                reportDate = x.Date.ToString("yyyy/MM/dd HH:mm")
            });

            // Perform filtering and sorting operations using the provided parameters.
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                result = result.Where(x => (x.reporterName.ToLower().Contains(searchValue)));
            }

            int totalRecords = result.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = result.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = result.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Obtem uma lista em json das denúncias que foram feitas contra as equipas deste utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>JSON com as equipas, nº reports e status</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetUserTeamsReports(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection, Guid id)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var teams = await _teamsService.GetTeams();

            teams = teams.Where(t => t.OwnerId.Value == id || t.Members.Any(m => m.Id == id.ToString())).ToList();

            var rawReports = await _adminService.GetUserTeamsReports(id);

            var archivedReports = await _reportReasonService.GetArchiveTeamReports();
            archivedReports = archivedReports.Where(x => x.ReportedTeam.OwnerId == id || x.ReportedTeam.Members.Any(m => m.Id == id.ToString())).ToList();
            foreach (ReportTeam r in archivedReports)
            {
                rawReports.Add(r);
            }

            var result = teams.Select(x => new
            {
                reportedTeamId = x.TeamId,
                reportedTeamName = x.TeamName,
                banned = x.IsBanned,
                totalReports = rawReports.Where(y => y.ReportedTeamId.Equals(x.TeamId)).Count()
            });

            // Perform filtering and sorting operations using the provided parameters.
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                result = result.Where(x => (x.reportedTeamName.ToLower().Contains(searchValue)));
            }

            int totalRecords = result.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = result.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = result.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Obtem uma lista em json das denúncias que foram feitas contra os eventos deste utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>JSON com os eventos, nº reports e status</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetUserEventsReports(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection, Guid id)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var user = await _userService.GetUser(id);
            var userEvents = user.Events;

            var rawReports = await _adminService.GetUserEventsReports(id);

            var archivedReports = await _reportReasonService.GetArchiveEventReports();
            archivedReports = archivedReports.Where(x => x.ReportedEvent.UserId == id.ToString()).ToList();
            foreach (ReportEvent r in archivedReports)
            {
                rawReports.Add(r);
            }

            var result = userEvents.Select(x => new
            {
                reportedEventId = x.EventId,
                reportedEventName = x.Title,
                banned = x.IsBanned,
                totalReports = rawReports.Where(y => y.ReportedEventId.Equals(x.EventId)).Count()
            });

            // Perform filtering and sorting operations using the provided parameters.
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                result = result.Where(x => (x.reportedEventName.ToLower().Contains(searchValue)));
            }

            int totalRecords = result.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = result.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = result.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Recebe o id do utilizador banido e envia um email para o mesmo.
        /// </summary>
        /// <param name="userId">Id do utilizador banido.</param>
        /// <returns>True: se bem sucedido. False: Se mal sucedido.</returns>
        private async Task<bool> SendUserBanEmail(Guid userId)
        {
            try
            {
                //Send email
                User bannedUser = await _userService.GetUser(userId);

                Task.Run(() => _emailService.SendBanUserEmail(bannedUser));
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Recebe o id do utilizador de que o banimento foi removido e envia um email para o mesmo.
        /// </summary>
        /// <param name="userId">Id do utilizador.</param>
        /// <returns>True: se bem sucedido. False: Se mal sucedido.</returns>
        private async Task<bool> SendUserUnbanEmail(Guid userId)
        {
            try
            {
                //Send email
                User bannedUser = await _userService.GetUser(userId);

                Task.Run(() => _emailService.SendUnbanUserEmail(bannedUser));
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }
    }
}
