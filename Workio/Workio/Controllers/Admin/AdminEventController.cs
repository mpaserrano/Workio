using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Data;
using Workio.Services.Admin.Events;
using Workio.Services.Interfaces;
using Workio.Services.RequestEntityStatusServices;
using Workio.Models;
using Microsoft.Extensions.Logging;
using Workio.Services.Admin.Log;
using Workio.Models.Events;
using Workio.Services.Events;
using Workio.Services.ReportServices;
using Workio.Services.Admin.Teams;
using Workio.Services;
using System.Web;

namespace Workio.Controllers.Admin
{
    public class AdminEventController : Controller
    {

        private readonly IWebHostEnvironment _webhostEnvironemnt;
        private readonly IToastNotification _toastNotification;
        private readonly IUserService _userService;
        private readonly IAdminEventService _adminEventService;
        private readonly ILogsService _logsService;
        private readonly IEventsService _eventsService;
        private readonly IReportReasonService _reportReasonService;
        private readonly CommonLocalizationService _localizerString;

        public AdminEventController(IWebHostEnvironment webHostEnvironment, IToastNotification toastNotification, IUserService userService, IAdminEventService adminEventService, ILogsService logsService, IEventsService eventsService, IReportReasonService reportReasonService,
            CommonLocalizationService commonLocalizationService)
        {
            _webhostEnvironemnt = webHostEnvironment;
            _toastNotification = toastNotification;
            _userService = userService;
            _adminEventService = adminEventService;
            _logsService = logsService;
            _eventsService = eventsService;
            _reportReasonService = reportReasonService;
            _localizerString = commonLocalizationService;
        }

        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.events = await _adminEventService.GetEvents();
            return View();
        }

        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == null || await _eventsService.GetEvent(id) == null)
            {
                return NotFound();
            }
            var @event = await _eventsService.GetEvent(id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        /// <summary>
        /// Muda o estado de featured do evento.
        /// </summary>
        /// <returns>Redireciona para a pagina do evento na administração</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> ChangeFeaturedStatus(Guid id, bool newStatus, LogViewModel logModel, string? returnUrl)
        {
            if (id == null) return NotFound();

            var success = false;

            if (newStatus)
                success = await _adminEventService.MarkAsFeatured(id);
            else
                success = await _adminEventService.RemoveFeatured(id);

            if (success)
            {
                var log = await _logsService.CreateEventActionLog(logModel.Description, id.ToString(), newStatus ? Models.Admin.Logs.EventActionLogType.MarkAsFeatured : Models.Admin.Logs.EventActionLogType.RemoveFeatured);
                _toastNotification.AddSuccessToastMessage(_localizerString.Get("SuccessChangingFeaturedStatus"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizerString.Get("FailedToChangeFeaturedStatus"));
            }

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }

        /// <summary>
        /// Bane um evento.
        /// </summary>
        /// <returns>redireciona para o Index</returns>
        [ActionName("BanEvent")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> BanEvent(string id, LogViewModel logModel, string returnUrl)
        {
            if(await _adminEventService.BanEvent(new Guid(id))){
                _toastNotification.AddSuccessToastMessage(_localizerString.Get("EventBanned"));

                if (logModel != null)
                {
                    await _logsService.CreateEventActionLog(logModel.Description, id, Models.Admin.Logs.EventActionLogType.Ban);
                }
                else
                {
                    await _logsService.CreateEventActionLog("Banned Event", id, Models.Admin.Logs.EventActionLogType.Ban);
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizerString.Get("EventBannedFail"));
            }

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Bane um evento da pagina de details.
        /// </summary>
        /// <returns>redireciona para a pagina de detalhes de administração do evento</returns>
        [ActionName("BanEventFromDetails")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> BanEventFromDetails(string id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();

            if (await _adminEventService.BanEvent(new Guid(id)))
            {
                _toastNotification.AddSuccessToastMessage(_localizerString.Get("EventBanned"));
                await _logsService.CreateEventActionLog(logModel.Description, id, Models.Admin.Logs.EventActionLogType.Ban);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizerString.Get("EventBannedFail"));
            }
            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Remove o ban de um evento.
        /// </summary>
        /// <returns>redireciona para o Index</returns>
        [ActionName("UnbanEvent")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> UnbanEvent(string id, LogViewModel logModel, string returnUrl)
        {
            if (await _adminEventService.UnbanEvent(new Guid(id)))
            {
                _toastNotification.AddSuccessToastMessage(_localizerString.Get("EventUnbanned"));

                if (logModel != null)
                {
                    await _logsService.CreateEventActionLog(logModel.Description, id, Models.Admin.Logs.EventActionLogType.Unban);
                }
                else
                {
                    await _logsService.CreateEventActionLog("Unbanned Event", id, Models.Admin.Logs.EventActionLogType.Unban);
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizerString.Get("EventUnbannedFail"));
            }

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Remove o ban de um evento da pagina de details.
        /// </summary>
        /// <returns>redireciona para a pagina de detalhes de administração do evento</returns>
        [ActionName("UnbanEventFromDetails")]
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> UnbanEventFromDetails(string id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();

            if (await _adminEventService.UnbanEvent(new Guid(id)))
            {
                _toastNotification.AddSuccessToastMessage(_localizerString.Get("EventUnbanned"));
                await _logsService.CreateEventActionLog(logModel.Description, id, Models.Admin.Logs.EventActionLogType.Unban);
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizerString.Get("EventUnbannedFail"));
            }


            return RedirectToAction("Details", new { id = id });
        }


        /// <summary>
        /// Obtem e trabalha todas os eventos para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetAllEventData(int draw, int start, int length, string searchValue)
        {
            var rawEvents = await _adminEventService.GetEvents();

            var events = rawEvents.Select(x => new
            {
                eventId = x.EventId,
                title = HttpUtility.HtmlEncode(x.Title),
                creatorId = x.UserId,
                creatorName = HttpUtility.HtmlEncode(_userService.GetUser(new Guid(x.UserId)).Result.Name),
                status = x.State,
                banned = x.IsBanned,
                startDate = x.StartDate.ToString("yyyy/MM/dd HH:mm"),
                endDate = x.EndDate.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                events = events.Where(x => (x.title.ToLower().Contains(searchValue)));
            }

            int totalRecords = events.Count();

            IEnumerable<Object> pagedData = events.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = events.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha todas os eventos abertos para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetOpenEventData(int draw, int start, int length, string searchValue)
        {
            var rawEvents = await _adminEventService.GetEvents();

            var events = rawEvents.Where(x => (x.State == Models.Events.EventState.Open) && (x.IsBanned == false)).Select(x => new
            {
                eventId = x.EventId,
                title = HttpUtility.HtmlEncode(x.Title),
                creatorId = x.UserId,
                creatorName = HttpUtility.HtmlEncode(_userService.GetUser(new Guid(x.UserId)).Result.Name),
                status = x.State,
                banned = x.IsBanned,
                startDate = x.StartDate.ToString("yyyy/MM/dd HH:mm"),
                endDate = x.EndDate.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                events = events.Where(x => (x.title.ToLower().Contains(searchValue)));
            }

            int totalRecords = events.Count();

            IEnumerable<Object> pagedData = events.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = events.Count(),
                data = pagedData
            };
            return Json(response);
        }


        /// <summary>
        /// Obtem e trabalha todas os eventos on going para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetOnGoingEventData(int draw, int start, int length, string searchValue)
        {
            var rawEvents = await _adminEventService.GetEvents();

            var events = rawEvents.Where(x => (x.State == Models.Events.EventState.OnGoing) && (x.IsBanned == false)).Select(x => new
            {
                eventId = x.EventId,
                title = HttpUtility.HtmlEncode(x.Title),
                creatorId = x.UserId,
                creatorName = HttpUtility.HtmlEncode(_userService.GetUser(new Guid(x.UserId)).Result.Name),
                status = x.State,
                banned = x.IsBanned,
                startDate = x.StartDate.ToString("yyyy/MM/dd HH:mm"),
                endDate = x.EndDate.ToString("yyyy/MM/dd HH:mm")
            });


            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                events = events.Where(x => (x.title.ToLower().Contains(searchValue)));
            }

            int totalRecords = events.Count();

            IEnumerable<Object> pagedData = events.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = events.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha todas os eventos que acabaram para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetFinishedEventData(int draw, int start, int length, string searchValue)
        {
            var rawEvents = await _adminEventService.GetEvents();

            var events = rawEvents.Where(x => (x.State == Models.Events.EventState.Finish) && (x.IsBanned == false)).Select(x => new
            {
                eventId = x.EventId,
                title = HttpUtility.HtmlEncode(x.Title),
                creatorId = x.UserId,
                creatorName = HttpUtility.HtmlEncode(_userService.GetUser(new Guid(x.UserId)).Result.Name),
                status = x.State,
                banned = x.IsBanned,
                startDate = x.StartDate.ToString("yyyy/MM/dd HH:mm"),
                endDate = x.EndDate.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                events = events.Where(x => (x.title.ToLower().Contains(searchValue)));
            }

            int totalRecords = events.Count();

            IEnumerable<Object> pagedData = events.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = events.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha todas os eventos que foram banidos para serem usadas na tabela.
        /// </summary>
        /// <param name="draw">draw</param>
        /// /// <param name="start">Inicio da tabela</param>
        /// /// <param name="length">Quantidades de equipas</param>
        /// /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetBannedEventData(int draw, int start, int length, string searchValue)
        {
            var rawEvents = await _adminEventService.GetEvents();

            var events = rawEvents.Where(x => x.IsBanned == true).Select(x => new
            {
                eventId = x.EventId,
                title = HttpUtility.HtmlEncode(x.Title),
                creatorId = x.UserId,
                creatorName = HttpUtility.HtmlEncode(_userService.GetUser(new Guid(x.UserId)).Result.Name),
                status = x.State,
                banned = x.IsBanned,
                startDate = x.StartDate.ToString("yyyy/MM/dd HH:mm"),
                endDate = x.EndDate.ToString("yyyy/MM/dd HH:mm")
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                events = events.Where(x => (x.title.ToLower().Contains(searchValue)));
            }

            int totalRecords = events.Count();

            IEnumerable<Object> pagedData = events.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = events.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Obtem e trabalha os reports de um certo evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <param name="draw">draw</param>
        /// <param name="start">Inicio da tabela</param>
        /// <param name="length">Quantidades de equipas</param>
        /// <param name="searchValue">Valor de pesquisa da tabela</param>
        /// <returns>Objeto JSON com as equipas</returns>
        [Authorize(Roles = "Admin, Mod")]
        [HttpGet]
        public async Task<IActionResult> GetEventReports(Guid id, int draw, int start, int length)
        {

            var rawReports = await _reportReasonService.GetEventReports();
            var archivedReports = await _reportReasonService.GetArchiveEventReports();
            foreach (ReportEvent r in archivedReports)
            {
                rawReports.Add(r);
            }

            var reports = rawReports.Where(x => x.ReportedEventId == id).Select(x => new
            {
                id = x.Id,
                reporterId = x.ReporterId,
                reporterName = HttpUtility.HtmlEncode(_userService.GetUser(new Guid(x.ReporterId)).Result.Name),
                reporterProfilePicture = _userService.GetUser(new Guid(x.ReporterId)).Result.ProfilePicture,
                reportReason = HttpUtility.HtmlEncode(x.ReportReason.Reason),
                reportStatus = x.ReportStatus,
                reportDate = x.Date.ToString("yyyy/MM/dd HH:mm"),
                reportedId = x.ReportedEventId
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


    }
}
