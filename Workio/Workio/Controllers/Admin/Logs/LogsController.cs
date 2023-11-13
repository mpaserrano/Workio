using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Workio.Data;
using Workio.Models;
using Workio.Models.Admin.Logs;
using Workio.Models.Events;
using Workio.Services.Admin.Log;
using Workio.Services.LocalizationServices;
using Workio.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Workio.Controllers.Admin.Logs
{
    /// <summary>
    /// Gerencia os acesso ás páginas relativamente aos logs
    /// </summary>
    public class LogsController : Controller
    {
        /// <summary>
        /// Servicos relativamente ao login do utilizador.
        /// </summary>
        private SignInManager<User> _signInManager;
        /// <summary>
        /// Serviço para a gestão de logs.
        /// </summary>
        private ILogsService _logsService;
        /// <summary>
        /// Serviço para obter informações dos utilizadores.
        /// </summary>
        private IUserService _userService;
        /// <summary>
        /// Interface para mandar notificações para o frontend.
        /// </summary>
        private readonly IToastNotification _toastNotification;
        /// <summary>
        /// Interface para a aplicação de traduções a enviar ao frontend.
        /// </summary>
        private readonly ILocalizationService _localizationService;

        public LogsController(SignInManager<User> signInManager,
            ILogsService logsService,
            IUserService userService,
            IToastNotification toastNotification,
            ILocalizationService localizationService)
        {
            _signInManager = signInManager;
            _logsService = logsService;
            _userService = userService;
            _toastNotification = toastNotification;
            _localizationService = localizationService;
        }

        // GET: Logs
        /// <summary>
        /// Retorna a página principal da listagem de logs.
        /// </summary>
        /// <returns>View da página principal de listagem.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        /// <summary>
        /// Recebe o id de um log de utilizador e retorna uma view com as informações
        /// do log.
        /// </summary>
        /// <param name="id">Id do log de utilizador. (UserActionLog)</param>
        /// <returns>View com informações do log se o id existir, senão erro 404.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> Details(string id)
        {
            var log = await _logsService.GetUserActionLogByLogId(id);
            if (id == null || log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        /// <summary>
        /// Recebe o id de um log de um aequipa e retorna uma view com as informações
        /// do log.
        /// </summary>
        /// <param name="id">Id do log da equipa. (TeamActionLog)</param>
        /// <returns>View com informações do log se o id existir, senão erro 404.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> TeamActionLogDetails(string id)
        {
            var log = await _logsService.GetTeamActionLogByLogId(id);

            if (id == null || log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        /// <summary>
        /// Recebe o id de um log de um evento e retorna uma view com as informações
        /// do log.
        /// </summary>
        /// <param name="id">Id do log do evento. (EventActionLog)</param>
        /// <returns>View com informações do log se o id existir, senão erro 404.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> EventActionLogDetails(string id)
        {
            var log = await _logsService.GetEventActionLogByLogId(id);

            if (id == null || log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        /// <summary>
        /// Recebe o id de um log de admnistração e retorna uma view com as informações
        /// do log.
        /// </summary>
        /// <param name="id">Id do log de administração. (AdministrationActionLog)</param>
        /// <returns>View com informações do log se o id existir, senão erro 404.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> AdministrationActionLogDetails(string id)
        {
            var log = await _logsService.GetAdministrationActionLogByLogId(id);

            if (id == null || log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        /// <summary>
        /// Retorna um json com a listagem de logs de utilizadores relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetUsersLogsData(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            //var usersLogData = await _logsService.GetUserActionLogData();
            var usersLogData = await _logsService.GetUserActionDataFiltered(searchValue);


            // Perform filtering and sorting operations using the provided parameters.

            // Calculate the total number of records that match the search criteria.
            int totalRecords = usersLogData.Count();

            // Apply pagination to the filtered data.
            IEnumerable<Object> pagedData = usersLogData.Skip(start).Take(length);

            // Construct the response object.
            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = usersLogData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Retorna um json com a listagem de logs de equipas relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetTeamsLogsData(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            var usersLogData = await _logsService.GetTeamActionLogDataFiltered(searchValue);

            int totalRecords = usersLogData.Count();

            IEnumerable<Object> pagedData = usersLogData.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = usersLogData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Retorna um json com a listagem de logs de eventos relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetEventsLogsData(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            var usersLogData = await _logsService.GetEventActionLogDataFiltered(searchValue);

            int totalRecords = usersLogData.Count();

            IEnumerable<Object> pagedData = usersLogData.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = usersLogData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }

        /// <summary>
        /// Retorna um json com a listagem de logs da administração relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetAdminLogsData(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            var usersLogData = await _logsService.GetAdminActionLogDataFiltered(searchValue);

            int totalRecords = usersLogData.Count();

            IEnumerable<Object> pagedData = usersLogData.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = usersLogData.Count(),
                data = pagedData
            };

            // Return the response as JSON.
            return Json(response);
        }
    }
}
