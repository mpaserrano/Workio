using MessagePack.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Composition;
using System.Security.Claims;
using Workio.Models;
using Workio.Services;
using Workio.Services.Admin.Log;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.ReportServices;
using Workio.Services.Teams;
using Workio.Utils;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador para a gestão das denuncias por parte da administração
    /// </summary>
    [Authorize(Roles = "Admin, Mod")]
    public class AdminReportsController : Controller
    {
        /// <summary>
        /// Serviço para obter denuncias
        /// </summary>
        private readonly IReportReasonService _reportReasonService;
        /// <summary>
        /// Serviço para obter os utilizadores
        /// </summary>
        private readonly IUserService _userService;
        /// <summary>
        /// Serviço para obter as equipas
        /// </summary>
        private readonly ITeamsService _teamsService;
        /// <summary>
        /// Serviço para obter os eventos
        /// </summary>
        private readonly IEventsService _eventsService;
        /// <summary>
        /// Objeto do tipo ToastNotification para as notificações das ações realizadas
        /// </summary>
        private readonly IToastNotification _toastNotification;
        private readonly int _pageSize = 10;
        /// <summary>
        /// Objeto do tipo ToastNotification para as notificações das ações realizadas
        /// </summary>
        private readonly ILogsService _logsService;
        /// <summary>
        /// Objeto do tipo CommonLocalizationService para a tradução das notificações das ações
        /// </summary>
        private readonly CommonLocalizationService _localizationService;
        /// <summary>
        /// Construtor da classe.
        /// </summary>
        ///<param name="reportReasonService">Parametro de Serviço de denuncias para inicialização</param>
        ///
        public AdminReportsController(IReportReasonService reportReasonService, IUserService userService, ITeamsService teamsService, IEventsService eventsService, IToastNotification toastNotification, ILogsService logsService, CommonLocalizationService localizationService)
        {
            _reportReasonService = reportReasonService;
            _userService = userService;
            _teamsService = teamsService;
            _eventsService = eventsService;
            _toastNotification = toastNotification;
            _logsService = logsService;
            _localizationService = localizationService;
        }
        /// <summary>
        /// Metodo de encaminhamento para a página de visualização das denuncias
        /// </summary>
        public async Task<IActionResult> Reports()
        {
            ICollection<ReportUser> userReports = await _reportReasonService.GetUserReports();
            ICollection<ReportTeam> teamReports = await _reportReasonService.GetTeamReports();
            ICollection<ReportEvent> eventReports = await _reportReasonService.GetEventReports();
            ViewBag.UserReports = userReports;
            ViewBag.TeamReports = teamReports;
            ViewBag.EventReports = eventReports;
            return View();
        }
        /// <summary>
        /// Metodo responsavel por mostrar a página de detalhes de uma denuncia de utilizador
        /// </summary>
        ///<param name="id">Id da denuncia realizada ao utilizador</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> DetailsUser(Guid id)
        {
            var report = await _reportReasonService.GetUserReport(id);
            if (report == null) { return NotFound(); }
            var allReports = await _reportReasonService.GetUserReports();
            ViewBag.OtherReports = from r in allReports where r.ReportedId == report.ReportedId && r.Id != report.Id select r;
            return View(report);
        }
        /// <summary>
        /// Metodo responsavel por mostrar a página de detalhes de uma denuncia de equipa
        /// </summary>
        ///<param name="id">Id da denuncia realizada à equipa</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> DetailsTeam(Guid id)
        {
            var report = await _reportReasonService.GetTeamReport(id);
            if (report == null) { return NotFound(); }
            var OwnerId = report.ReportedTeam.OwnerId;
            var allReports = await _reportReasonService.GetTeamReports();
            ViewBag.OtherReports = from r in allReports where r.ReportedTeamId == report.ReportedTeamId && r.Id != report.Id select r;
            return View(report);
        }
        /// <summary>
        /// Metodo responsavel por mostrar a página de detalhes de uma denuncia de evento
        /// </summary>
        ///<param name="id">Id da denuncia realizada ao evento</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> DetailsEvent(Guid id)
        {
            var report = await _reportReasonService.GetEventReport(id);
            if (report == null) { return NotFound(); }
            var allReports = await _reportReasonService.GetEventReports();
            ViewBag.OtherReports = from r in allReports where r.ReportedEventId == report.ReportedEventId && r.Id != report.Id select r;
            return View(report);
        }

        /// <summary>
        /// Metodo responsavel por mostrar a página de adicionar um nova razão de denuncia
        /// </summary>
        ///<param name="id">Id da denuncia realizada ao evento</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> AddReportReason()
        {
            List<SelectListItem> reasonTypes = new List<SelectListItem>()
            {
                new SelectListItem { Value = ReasonType.User.ToString(), Text = TranslateReasonType(ReasonType.User) },
                new SelectListItem { Value = ReasonType.Team.ToString(), Text = TranslateReasonType(ReasonType.Team) },
                new SelectListItem { Value = ReasonType.Event.ToString(), Text = TranslateReasonType(ReasonType.Event) }
            };
            ViewBag.ReasonTypes = new SelectList(reasonTypes, "Value", "Text");
            return View();
        }


        /// <summary>
        /// Metodo responsavel por rejeitar uma denuncia de utilizador
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        ///<param name="logModel">Modal com a justificação da ação para os logs</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> RejectUserReport(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            var result = await _reportReasonService.RejectUserReport(id);
            var user = await _userService.GetUser(CurrentUserId());
            if (result)
            {
                await _logsService.CreateUserActionLog(logModel.Description, user.Id.ToString(), Models.Admin.Logs.UserActionLogType.Other);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("ReportRejected"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedReportRejected"));
            }
            return RedirectToAction("Reports");
        }
        /// <summary>
        /// Metodo responsavel por rejeitar uma denuncia de equipa
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        ///<param name="logModel">Modal com a justificação da ação para os logs</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> RejectTeamReport(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            var result = await _reportReasonService.RejectTeamReport(id);
            var user = await _userService.GetUser(CurrentUserId());
            var report = await _reportReasonService.GetTeamReport(id);
            if (result)
            {
                await _logsService.CreateTeamActionLog(logModel.Description, report.ReportedTeamId.ToString(), Models.Admin.Logs.TeamActionLogType.Other);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("ReportRejected"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedReportRejected"));
            }
            return RedirectToAction("Reports");
        }
        /// <summary>
        /// Metodo responsavel por rejeitar uma denuncia de evento
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        ///<param name="logModel">Modal com a justificação da ação para os logs</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> RejectEventReport(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            var result = await _reportReasonService.RejectEventReport(id);
            var user = await _userService.GetUser(CurrentUserId());
            var report = await _reportReasonService.GetEventReport(id);
            if (result)
            {
                await _logsService.CreateEventActionLog(logModel.Description, report.ReportedEventId.ToString(), Models.Admin.Logs.EventActionLogType.Other);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("ReportRejected"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedReportRejected"));
            }
            return RedirectToAction("Reports");
        }
        /// <summary>
        /// Metodo responsavel por aceitar uma denuncia de utilizador
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        ///<param name="logModel">Modal com a justificação da ação para os logs</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> AcceptUserReport(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            var result = await _reportReasonService.AcceptUserReport(id);
            var user = await _userService.GetUser(CurrentUserId());
            if (result)
            {
                await _logsService.CreateUserActionLog(logModel.Description, user.Id.ToString(), Models.Admin.Logs.UserActionLogType.Other);
                await _logsService.CreateUserActionLog("User was banned from the report " + id, user.Id.ToString(), Models.Admin.Logs.UserActionLogType.Banned);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("UserBanned"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedAcceptReport"));
            }
            return RedirectToAction("Reports");
        }
        /// <summary>
        /// Metodo responsavel por aceitar uma denuncia de equipa
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        ///<param name="logModel">Modal com a justificação da ação para os logs</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> AcceptTeamReport(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            var result = await _reportReasonService.AcceptTeamReport(id);
            var report = await _reportReasonService.GetTeamReport(id);
            if (result)
            {
                await _logsService.CreateTeamActionLog(logModel.Description, report.ReportedTeamId.ToString(), Models.Admin.Logs.TeamActionLogType.Other);
                await _logsService.CreateTeamActionLog("Team was banned from the report " + id.ToString(), report.ReportedTeamId.ToString(), Models.Admin.Logs.TeamActionLogType.Hidden);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("TeamBanned"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedAcceptReport"));
            }
            return RedirectToAction("Reports");
        }
        /// <summary>
        /// Metodo responsavel por aceitar uma denuncia de evento
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        ///<returns>Retorna a view </returns>
        public async Task<IActionResult> AcceptEventReport(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();
            var result = await _reportReasonService.AcceptEventReport(id);
            var report = await _reportReasonService.GetEventReport(id);
            if (result)
            {
                await _logsService.CreateEventActionLog(logModel.Description, report.ReportedEventId.ToString(), Models.Admin.Logs.EventActionLogType.Other);
                await _logsService.CreateEventActionLog("Event was banned from the report " + id, report.ReportedEventId.ToString(), Models.Admin.Logs.EventActionLogType.Ban);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("EventBanned"));
            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedAcceptReport"));
            }
            return RedirectToAction("Reports");
        }

        /// <summary>
        /// Metodo responsavel por devolver o json com as denuncias de utilizadores
        /// </summary>
        ///<param name="draw">Id da denuncia</param>
        ///<param name="start">Página que a tabela inicia</param>
        ///<param name="length">Numero de </param>
        ///<returns>Retorna o ficheiro com as denuncias de utilizador</returns>
        public async Task<IActionResult> GetDataUserReports(int draw, int start, int length)
        {
            var userReportData = await _reportReasonService.GetUserReports();


            // Perform filtering and sorting operations using the provided parameters.

            // Calculate the total number of records that match the search criteria.
            int totalRecords = userReportData.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = userReportData.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = userReportData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }
        /// <summary>
        /// Metodo responsavel por devolver o json com as denuncias de equipas
        /// </summary>
        ///<param name="draw">Id da denuncia</param>
        ///<param name="start">Página que a tabela inicia</param>
        ///<param name="length">Numero de </param>
        ///<returns>Retorna o ficheiro com as denuncias de equipas</returns>
        public async Task<IActionResult> GetDataTeamReports(int draw, int start, int length)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var teamReportData = await _reportReasonService.GetTeamReports();


            // Perform filtering and sorting operations using the provided parameters.

            // Calculate the total number of records that match the search criteria.
            int totalRecords = teamReportData.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = teamReportData.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = teamReportData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }
        /// <summary>
        /// Metodo responsavel por devolver o json com as denuncias dos eventos
        /// </summary>
        ///<param name="draw">Id da denuncia</param>
        ///<param name="start">Página que a tabela inicia</param>
        ///<param name="length">Numero de </param>
        ///<returns>Retorna o ficheiro com as denuncias dos eventos</returns>
        public async Task<IActionResult> GetDataEventReports(int draw, int start, int length)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var eventReportData = await _reportReasonService.GetEventReports();


            // Perform filtering and sorting operations using the provided parameters.

            // Calculate the total number of records that match the search criteria.
            int totalRecords = eventReportData.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = eventReportData.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = eventReportData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }
        /// <summary>
        /// Metodo responsavel por devolver o json com as denuncias resolvidas
        /// </summary>
        ///<param name="draw">Id da denuncia</param>
        ///<param name="start">Página que a tabela inicia</param>
        ///<param name="length">Numero de </param>
        ///<returns>Retorna o ficheiro com as denuncias resolvidas</returns>
        public async Task<IActionResult> GetDataArchiveReports(int draw, int start, int length)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var archiveReportData = await _reportReasonService.GetArchiveReports();


            // Perform filtering and sorting operations using the provided parameters.

            // Calculate the total number of records that match the search criteria.
            int totalRecords = archiveReportData.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = archiveReportData.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = archiveReportData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Metodo responsavel por devolver o json com as razões para denuncia
        /// </summary>
        ///<param name="draw">Id da denuncia</param>
        ///<param name="start">Página que a tabela inicia</param>
        ///<param name="length">Numero de </param>
        ///<returns>Retorna o ficheiro com as razões para denuncia</returns>
        public async Task<IActionResult> GetDataReportReasons(int draw, int start, int length)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var reportReasonsData = await _reportReasonService.GetReportReasonsAsync();


            // Perform filtering and sorting operations using the provided parameters.

            // Calculate the total number of records that match the search criteria.
            int totalRecords = reportReasonsData.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = reportReasonsData.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = reportReasonsData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Metodo responsavel por remover uma razão de denuncia
        /// </summary>
        ///<param name="id">Id da razão de denuncia a remover</param>
        ///<param name="logModel"></param>
        ///<returns>Retorna json com um bool que representa se o adicionar teve sucesso, true se teve sucee, false se não</returns>
        public async Task<IActionResult> RemoveReportReason(Guid id, LogViewModel logModel)
        {
            if (id == null || logModel == null || string.IsNullOrEmpty(logModel.Description)) return NotFound();

            var result = await _reportReasonService.RemoveReportReason(id);
            if (result)
            {
                await _logsService.CreateAdminActionLog(logModel.Description, Models.Admin.Logs.AdministrationActionLogType.Other);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("ReasonRemoved"));

            }
            else
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedReasonRemoved"));

            }
            return RedirectToAction("ReportReasons");

        }

        /// <summary>
        /// Metodo responsavel por adicionar um nova razão de denuncia
        /// </summary>
        ///<param name="formData">Informação do formario para adiconar uma razão de denuncia e a descrição para os logs</param>
        ///<returns>Retorna json com um bool que representa se o adicionar teve sucesso, true se teve sucee, false se não</returns>
        [HttpPost]
        public async Task<IActionResult> AddReportReason([FromBody] FormDataModel formData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            bool result;
            ReasonType reasonType;
            try
            {
                reasonType = (ReasonType)Enum.Parse(typeof(ReasonType), formData.ReasonType);
                ReportReason newReason = new ReportReason() { Id = Guid.NewGuid(), Reason = formData.Reason, ReasonType = reasonType };
                ReportReasonLocalization reportReasonLocalization = new ReportReasonLocalization() { Id = Guid.NewGuid(), ReportId = newReason.Id.Value, Description = formData.ReasonPortugues, LocalizationCode = "pt" };
                result = await _reportReasonService.AddNewReason(newReason);
                if (result)
                {
                    await _reportReasonService.AddReasonLocalization(reportReasonLocalization);
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            if (result)
            {
                await _logsService.CreateAdminActionLog(formData.Description, Models.Admin.Logs.AdministrationActionLogType.Other);
            }

            return Json(new { success = result });
        }

        /// <summary>
        /// Metodo responsavel por mostrar a página que lista as razões para denuncia
        /// </summary>
        ///<param name="success">Bool nullable para se o pedidio para adicionar uma nova razão para denuncia teve sucesso</param>
        ///<param name="error">Bool nullable para se o pedidio para adicionar uma nova razão para denuncia teve insucesso</param>
        ///<returns>Retorna a view com a lista de razões para denuncia</returns>
        public IActionResult ReportReasons(bool? success, bool? error)
        {
            bool successNoti = success.HasValue ? success.Value : false;
            bool errorNoti = error.HasValue ? error.Value : false;
            if (successNoti)
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("ReasonAdded"));
            }
            else if (errorNoti)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedReasonAdded"));
            }
            return View();

        }
        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// Traduzir o enumerado das razões
        /// </summary>
        /// <param name="reasonType">Tipo da razão de denúncia</param>
        /// <returns>Tradução da razão</returns>
        private string TranslateReasonType(ReasonType reasonType)
        {
            switch (reasonType)
            {
                case ReasonType.User:
                    return _localizationService.Get("User");
                case ReasonType.Team:
                    return _localizationService.Get("Team");
                case ReasonType.Event:
                    return _localizationService.Get("Event");
                default:
                    return "";
            }
        }
    }
}
