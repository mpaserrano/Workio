using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.ReportServices;
using Workio.Services.Teams;
using Workio.Services.Events;
using NToastNotify;
using Workio.Services;
using Microsoft.AspNetCore.Authorization;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador para as denuncias.
    /// </summary>
    [Authorize]
    public class ReportsController : Controller
    {
        /// <summary>
        /// Serviço para intereações com a base de dados relacionadas com denuncias
        /// </summary>
        private readonly IReportReasonService _reportReasonService;
        /// <summary>
        /// Serviço para intereações com a base de dados relacionadas as equipas
        /// </summary>
        private readonly ITeamsService _teamsService;
        /// <summary>
        /// Serviço para intereações com a base de dados relacionadas aos eventos
        /// </summary>
        private readonly IEventsService _eventsService;
        /// <summary>
        /// Objeto do tipo ToastNotification para as notificações das ações realizadas
        /// </summary>
        private readonly IToastNotification _toastNotification;
        /// <summary>
        /// Serviço para informações relacionadas à localização
        /// </summary>
        private readonly CommonLocalizationService _localizationService;

        /// <summary>
        /// Construtor da classe
        ///<param name="toastNotification">Objeto do tipo ToastNotification para as notificações das ações realizadas</param>
        ///<param name="eventsService">Serviço para intereações com a base de dados relacionadas aos eventos</param>
        ///<param name="reportService">Serviço para intereações com a base de dados relacionadas com as denuncias</param>
        ///<param name="teamsService">Serviço para intereações com a base de dados relacionadas às equipas</param>
        ///<param name="localizationService">Serviço para informações relacionadas à localização</param>
        /// </summary>
        public ReportsController(IReportReasonService reportService, ITeamsService teamsService, IEventsService eventsService, IToastNotification toastNotification, CommonLocalizationService localizationService)
        {
            _reportReasonService = reportService;
            _teamsService = teamsService;
            _eventsService = eventsService;
            _toastNotification = toastNotification;
            _localizationService = localizationService;
        }
        /// <summary>
        /// Metodo privado para carregar as razões de denuncias de utilizadores
        /// </summary>
        private async Task loadReasonsUser()
        {
            var reasons = await _reportReasonService.GetReportReasonsUserAsync();

            ViewBag.ReportReasons = new SelectList(reasons, "Id", "Reason");
        }

        /// <summary>
        /// Metodo privado para carregar as razões de denuncias de equipas
        /// </summary>
        private async Task loadReasonsTeam()
        {
            var reasons = await _reportReasonService.GetReportReasonsTeamAsync();

            ViewBag.ReportReasons = new SelectList(reasons, "Id", "Reason");
        }
        /// <summary>
        /// Metodo privado para carregar as razões de denuncias de eventos
        /// </summary>
        private async Task loadReasonsEvent()
        {
            var reasons = await _reportReasonService.GetReportReasonsEventAsync();

            ViewBag.ReportReasons = new SelectList(reasons, "Id", "Reason");
        }

        /// <summary>
        /// Ação para encaminhar para a página de report de utilizador
        /// </summary>
        /// <param name="id">Id do utilizador a ser denunciado</param>
        /// <returns>Task<IActionResult> - Redireciona para a página de denuncia de utilizadores</returns>
        public async Task<IActionResult> ReportUser(Guid id)
        {
            await loadReasonsUser();
            ViewBag.ReportedId = id;
            
            return View();
        }

        /// <summary>
        /// Ação para encaminhar para a página de report de equipa
        /// </summary>
        /// <param name="id">Id do utilizador a ser denunciado</param>
        /// <returns>Task<IActionResult> - Redireciona para a página de denuncia de equipas</returns>
        public async Task<IActionResult> ReportTeam(Guid id)
        {
            await loadReasonsTeam();
            ViewBag.ReportedId = id;

            var team = await _teamsService.GetTeamById(id);

            ViewBag.TeamName = team?.TeamName ?? string.Empty;
            Console.WriteLine(team.TeamName);
            return View();
        }

        public async Task<IActionResult> ReportEvent(Guid id)
        {
            await loadReasonsEvent();
            ViewBag.ReportedId = id;

            return View();
        }

        /// <summary>
        /// Ação "POST" da denuncia de equipas.
        /// </summary>
        /// <param name="reportEvent">Objeto com as informações sobre a denuncia do evento</param>
        /// <param name="id">Id do evento a ser denunciado</param>
        /// <returns>Task<IActionResult> - Redireciona para a pagina de confirmação de denuncia</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportEvent(ReportEvent reportEvent, Guid id)
        {
            await loadReasonsUser();
            if (reportEvent.ReportReasonId == Guid.Empty)
            {
                ModelState.AddModelError(String.Empty, _localizationService.Get("FillAllFields"));
            }
            if (ModelState.IsValid)
            {

                reportEvent.Id = Guid.NewGuid();
                reportEvent.ReporterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                reportEvent.ReportedEventId = id;
                reportEvent.Date = DateTime.Now;
                var result = await _reportReasonService.AddEventReport(reportEvent);
                if (result)
                {
                    return View("ThankYouForReport");

                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizationService.Get("ReportSendFail"));
                    return RedirectToAction("Details", "Events", new { id = id});
                }
            }
            return View(reportEvent);
        }

        /// <summary>
        /// Ação "POST" da denuncia de equipas.
        /// </summary>
        /// <param name="reportTeam">Objeto com as informações sobre a denuncia da equipa</param>
        /// <param name="id">Id da equipa a ser denunciada</param>
        /// <returns>Task<IActionResult> - Redireciona para a pagina de confirmação de denuncia</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportTeam(ReportTeam reportTeam, Guid id)
        {
            await loadReasonsUser();
            if (reportTeam.ReportReasonId == Guid.Empty)
            {
                ModelState.AddModelError(String.Empty, _localizationService.Get("FillAllFields"));
            }
            if (ModelState.IsValid)
            {

                reportTeam.Id = Guid.NewGuid();
                reportTeam.ReporterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                reportTeam.ReportedTeamId = id;
                reportTeam.Date = DateTime.Now;
                var result = await _reportReasonService.AddTeamReport(reportTeam);
                if (result)
                {
                    return View("ThankYouForReport");

                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizationService.Get("ReportSendFail"));
                    return RedirectToAction("Details", "Teams", new { id = id });
                }
            }
            return View(reportTeam);
        }

        /// <summary>
        /// Ação "POST" da denuncia de utilizadores.
        /// </summary>
        /// <param name="reportUser">Objeto com as informações sobre a denuncia do utilizador</param>
        /// <param name="id">Id do utilizador a ser denunciado</param>
        /// <returns>Task<IActionResult> - Mantem a mesma página de denuncia de utilizador</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportUser( ReportUser reportUser, Guid id)
        {
            await loadReasonsUser();
            if (reportUser.ReportReasonId == Guid.Empty)
            {
                ModelState.AddModelError(String.Empty, _localizationService.Get("FillAllFields"));
            }
            if (ModelState.IsValid)
            {

                reportUser.Id = Guid.NewGuid();
                reportUser.ReporterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                reportUser.ReportedId = id.ToString();
                reportUser.Date = DateTime.Now;
                if (reportUser.ReportedId == reportUser.ReporterId)
                {
                    _toastNotification.AddErrorToastMessage(_localizationService.Get("ReportSendFail"));
                    return RedirectToAction("Index", "User", new { id = reportUser.ReportedId });
                }
                var result = await _reportReasonService.AddUserReport(reportUser);
                if (result)
                {
                    return View("ThankYouForReport");

                }
                else
                {
                    _toastNotification.AddErrorToastMessage(_localizationService.Get("ReportSendFail"));
                    return RedirectToAction("Index", "User", new { id = id });
                }
            }
            return View(reportUser);
        }
        /// <summary>
        /// Metodo privado para verificar se um evento existe
        /// </summary>
        /// <param name="id">Id do evento a ser verificado</param>
        /// <returns>Devolve True se o evento existir, devolve False se não existir</returns>
        private bool ReportUserExists(Guid id)
        {
          return _eventsService.GetEvent(id) != null;
        }
    }
}
