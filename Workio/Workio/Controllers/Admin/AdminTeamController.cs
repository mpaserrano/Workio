using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Data;
using Workio.Services.Admin.Teams;
using Workio.Services.Interfaces;
using Workio.Services.RequestEntityStatusServices;
using Workio.Models;
using Workio.Services.Admin.Log;
using Workio.Services.Teams;
using Workio.Services.Admin.Events;
using Workio.Services.ReportServices;
using Org.BouncyCastle.Utilities.IO;
using Workio.Services;
using System.Web;

namespace Workio.Controllers.Admin
{
    public class AdminTeamController : Controller
    {

        private readonly IWebHostEnvironment _webhostEnvironemnt;
        private readonly IToastNotification _toastNotification;
        private readonly IUserService _userService;
        private readonly IAdminTeamService _adminTeamService;
        private readonly ILogsService _logsService;
        private readonly ITeamsService _teamsService; 
        private readonly IReportReasonService _reportReasonService;
        private readonly CommonLocalizationService _localizationService;

        public AdminTeamController(IWebHostEnvironment webHostEnvironment, IToastNotification toastNotification, IUserService userService, IAdminTeamService adminTeamService, ILogsService logsService, ITeamsService teamsService, IReportReasonService reportReasonService, CommonLocalizationService localizationService)
        {
            _webhostEnvironemnt = webHostEnvironment;
            _toastNotification = toastNotification;
            _userService = userService;
            _adminTeamService = adminTeamService;
            _logsService = logsService;
            _teamsService= teamsService;
            _reportReasonService = reportReasonService;
            _localizationService = localizationService;
        }

        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null || await _teamsService.GetTeamById(id) == null)
            {
                return NotFound();
            }
            var @event = await _teamsService.GetTeamById(id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        /// <summary>
        /// Bane uma equipa.
        /// </summary>
        /// <returns>redireciona para o Index</returns>
        [ActionName("BanTeam")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> BanTeam(string id, LogViewModel logModel)
        {
            if (logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();

            if (await _adminTeamService.BanTeam(new Guid(id)))
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("TeamBanned"));
                await _logsService.CreateTeamActionLog(logModel.Description, id, Models.Admin.Logs.TeamActionLogType.Ban);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("TeamBannedFail"));
            }
            return RedirectToAction("Index");
        }


        /// <summary>
        /// Bane uma equipa da pagina de details.
        /// </summary>
        /// <returns>redireciona para a pagina de detalhes de administração da equipa</returns>
        [ActionName("BanTeamFromDetails")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> BanTeamFromDetails(string id, LogViewModel logModel)
        {
            if (logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            if (await _adminTeamService.BanTeam(new Guid(id)))
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("TeamBanned"));
                await _logsService.CreateTeamActionLog(logModel.Description, id, Models.Admin.Logs.TeamActionLogType.Ban);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("TeamBannedFail"));
            }
            return RedirectToAction("Details", new {id = id});
        }

        /// <summary>
        /// Remove o ban de uma equipa.
        /// </summary>
        /// <returns>redireciona para o Index</returns>
        [ActionName("UnbanTeam")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> UnbanTeam(string id, LogViewModel logModel)
        {
            if (logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            if (await _adminTeamService.UnbanTeam(new Guid(id)))
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("TeamUnbanned"));
                await _logsService.CreateTeamActionLog(logModel.Description, id, Models.Admin.Logs.TeamActionLogType.Unban);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("TeamUnbannedFail"));
            }


            return RedirectToAction("Index");
        }

        /// <summary>
        /// Remove o ban de uma equipa da pagina de details.
        /// </summary>
        /// <returns>redireciona para a pagina de detalhes de administração da equipa</returns>
        [ActionName("UnbanTeamFromDetails")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> UnbanTeamFromDetails(string id, LogViewModel logModel)
        {
            if (logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            if (await _adminTeamService.UnbanTeam(new Guid(id)))
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("TeamUnbanned"));
                await _logsService.CreateTeamActionLog(logModel.Description, id, Models.Admin.Logs.TeamActionLogType.Unban);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("TeamUnbannedFail"));
            }


            return RedirectToAction("Details", new { id = id });
        }


        /// <summary>
        /// Obtem e trabalha todas as equipas para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetAllTeamData(int draw, int start, int length, string searchValue)
        {
            var rawTeams = await _adminTeamService.GetTeams();

            var teams = rawTeams.Select(x => new
            {
                teamId = x.TeamId,
                teamName = HttpUtility.HtmlEncode(x.TeamName),
                ownerId = x.OwnerId,
                ownerName = HttpUtility.HtmlEncode(_userService.GetUser(x.OwnerId.Value).Result.Name),
                status = x.Status,
                banned = x.IsBanned,
                createdAt = x.CreatedAt.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                teams = teams.Where(x => (x.teamName.ToLower().Contains(searchValue)));
            }

            int totalRecords = teams.Count();

            IEnumerable<Object> pagedData = teams.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = teams.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha as equipas abertas para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetOpenTeamData(int draw, int start, int length, string searchValue)
        {
            var rawTeams = await _adminTeamService.GetTeams();

            var teams = rawTeams.Where(x => (x.Status == TeamStatus.Open) && (x.IsBanned == false)).Select(x => new
            {
                teamId = x.TeamId,
                teamName = HttpUtility.HtmlEncode(x.TeamName),
                ownerId = x.OwnerId,
                ownerName = HttpUtility.HtmlEncode(_userService.GetUser(x.OwnerId.Value).Result.Name),
                status = x.Status,
                banned = x.IsBanned,
                createdAt = x.CreatedAt.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                teams = teams.Where(x => (x.teamName.ToLower().Contains(searchValue)));
            }

            int totalRecords = teams.Count();

            IEnumerable<Object> pagedData = teams.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = teams.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha as equipas fechadas para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetClosedTeamData(int draw, int start, int length, string searchValue)
        {
            var rawTeams = await _adminTeamService.GetTeams();

            var teams = rawTeams.Where(x => (x.Status == TeamStatus.Closed) && (x.IsBanned == false)).Select(x => new
            {
                teamId = x.TeamId,
                teamName = HttpUtility.HtmlEncode(x.TeamName),
                ownerId = x.OwnerId,
                ownerName = HttpUtility.HtmlEncode(_userService.GetUser(x.OwnerId.Value).Result.Name),
                status = x.Status,
                banned = x.IsBanned,
                createdAt = x.CreatedAt.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                teams = teams.Where(x => (x.teamName.ToLower().Contains(searchValue)));
            }

            int totalRecords = teams.Count();

            IEnumerable<Object> pagedData = teams.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = teams.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha as equipas acabadas para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetFinishedTeamData(int draw, int start, int length, string searchValue)
        {
            var rawTeams = await _adminTeamService.GetTeams();

            var teams = rawTeams.Where(x => (x.Status == TeamStatus.Finish) && (x.IsBanned == false)).Select(x => new
            {
                teamId = x.TeamId,
                teamName = HttpUtility.HtmlEncode(x.TeamName),
                ownerId = x.OwnerId,
                ownerName = HttpUtility.HtmlEncode(_userService.GetUser(x.OwnerId.Value).Result.Name),
                status = x.Status,
                banned = x.IsBanned,
                createdAt = x.CreatedAt.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                teams = teams.Where(x => (x.teamName.ToLower().Contains(searchValue)));
            }

            int totalRecords = teams.Count();

            IEnumerable<Object> pagedData = teams.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = teams.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha as equipas banidas para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetBannedTeamData(int draw, int start, int length, string searchValue)
        {
            var rawTeams = await _adminTeamService.GetTeams();

            var teams = rawTeams.Where(x => x.IsBanned == true).Select(x => new
            {
                teamId = x.TeamId,
                teamName = HttpUtility.HtmlEncode(x.TeamName),
                ownerId = x.OwnerId,
                ownerName = HttpUtility.HtmlEncode(_userService.GetUser(x.OwnerId.Value).Result.Name),
                status = x.Status,
                banned = x.IsBanned,
                createdAt = x.CreatedAt.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                teams = teams.Where(x => (x.teamName.ToLower().Contains(searchValue)));
            }

            int totalRecords = teams.Count();

            IEnumerable<Object> pagedData = teams.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = teams.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha os reports de uma certa team
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <param name="draw">draw</param>
        /// <param name="start">Inicio da tabela</param>
        /// <param name="length">Quantidades de equipas</param>
        /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetTeamReports(Guid id, int draw, int start, int length)
        {

            var rawReports = await _reportReasonService.GetTeamReports();
            var archivedReports = await _reportReasonService.GetArchiveTeamReports();
            foreach(ReportTeam r in archivedReports)
            {
                rawReports.Add(r);
            }

            var reports = rawReports.Where(x => x.ReportedTeamId == id).Select(x => new
            {
                id = x.Id,
                reporterId= x.ReporterId,
                reporterName = HttpUtility.HtmlEncode(_userService.GetUser(new Guid(x.ReporterId)).Result.Name),
                reporterProfilePicture = _userService.GetUser(new Guid(x.ReporterId)).Result.ProfilePicture,
                reportReason = HttpUtility.HtmlEncode(x.ReportReason.Reason),
                reportStatus = x.ReportStatus,
                reportDate = x.Date.ToString("yyyy/MM/dd HH:mm"),
                reportedId = x.ReportedTeamId
            });

            int totalRecords = reports.Count();

            IEnumerable<Object> pagedData = reports.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = reports.Count(),
                data = pagedData
            };

            return Json(response);
        }


        /// <summary>
        /// Obtem os membros e os reports de uma equipa para usar na tabela
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetTeamMemberReports(Guid id, int draw, int start, int length, string searchValue)
        {
            var team = await _teamsService.GetTeamById(id);

            var teamMembers = team.Members;
            teamMembers.Add(await _userService.GetUser(team.OwnerId.Value));

            var rawReports = await _reportReasonService.GetUserReports();
            var archivedReports = await _reportReasonService.GetArchiveUserReports();

            foreach (ReportUser r in archivedReports)
            {
                rawReports.Add(r);
            }


            var members = teamMembers.Select(x => new
            {
                userId = x.Id,
                userName = HttpUtility.HtmlEncode(x.Name),
                userProfilePicture = x.ProfilePicture,
                banned = (x.LockoutEnd != null),
                totalReports = rawReports.Where(y => y.ReportedId.Equals(x.Id)).Count()
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                members = members.Where(x => (x.userName.ToLower().Contains(searchValue)));
            }


            int totalRecords = members.Count();

            IEnumerable<Object> pagedData = members.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = members.Count(),
                data = pagedData
            };
            return Json(response);
        }

    }
}
