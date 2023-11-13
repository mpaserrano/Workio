using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using Workio.Models;
using Workio.Services;
using Workio.Services.Admin;
using Workio.Services.Admin.Log;
using Workio.Services.Interfaces;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador que dá handle da página da administração para gerir os pedidos de entidade certificada
    /// </summary>
    public class AdminEntitiesController : Controller
    {
        /// <summary>
        /// Para receber, processar e responder pedidos relativamente á manipulação de pedidos de entidade registada - admin
        /// </summary>
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly IToastNotification _toastNotification;
        private readonly ILogsService _logsService;
        private readonly CommonLocalizationService _localizationService;

        /// <summary>
        /// Constructor da classe.
        /// </summary>
        /// <param name="adminService">Variavel para aceder ao serviço de admin</param>
        /// <param name="userService">Variavel para aceder ao serviço de utilizadores</param>
        public AdminEntitiesController(IAdminService adminService, IToastNotification toastNotification, IUserService userService, ILogsService logsService, CommonLocalizationService localizationService)
        {
            _adminService = adminService;
            _toastNotification = toastNotification;
            _userService = userService;
            _logsService = logsService;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Ação para aprovar um pedido de entidade registada
        /// </summary>
        /// <param name="id">Variavel com o id do pedido</param>
        /// <returns>Task<IActionResult> - Redireciona para o detalhe do pedido com notificação bem sucedida</returns
        /// 
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> ApproveRequest(Guid id)
        {
            var result = await _adminService.ApproveRequest(id);
            var user = await GetUserRequested(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedApproveRequest"));
                RedirectToAction("Details", "RequestEntityStatus", new { id = id });
            }
            else
            {
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("ApproveRequest"));
                await _logsService.CreateUserActionLog("Accepted Request", user.Id.ToString(), Models.Admin.Logs.UserActionLogType.GiveEntity);
                RedirectToAction("Details", "RequestEntityStatus", new { id = id });
            }
            return RedirectToAction("Details", "RequestEntityStatus", new { id = id });
        }
        /// <summary>
        /// Ação para rejeitar um pedido de entidade registada
        /// </summary>
        /// <param name="id">Variavel com o id do pedido</param>
        /// <returns>Task<IActionResult> - Redireciona para o detalhe do pedido com notificação bem sucedida</returns
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> RejectRequest(Guid id)
        {
            var user = await GetUserRequested(id);
            var result = await _adminService.RejectRequest(id);
            if (result == false)
            {
                _toastNotification.AddErrorToastMessage(_localizationService.Get("FailedRejectRequest"));
                RedirectToAction("Details", "RequestEntityStatus", new { id = id });
            }
            else
            {
                await _logsService.CreateUserActionLog("Rejected Request", user.Id, Models.Admin.Logs.UserActionLogType.Other);
                _toastNotification.AddSuccessToastMessage(_localizationService.Get("RejectRequest"));
                RedirectToAction("Details", "RequestEntityStatus", new { id = id });
            }
            return RedirectToAction("Details", "RequestEntityStatus", new { id = id });
        }
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> EntityRequestsIndex()
        {
            return View();
        }
        /// <summary>
        /// Retorna um json com a listagem de logs de pedidos relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetRequestAll(int draw, int start, int length, string searchValue)
        {
            var requestData = await _adminService.GetRequests();

            var requests = requestData.Select(r => new
            {
                id = r.Id,
                userId = r.UserId,
                username = _userService.GetUser(r.UserId.Value).Result.Name,
                email = _userService.GetUser(r.UserId.Value).Result.Email,
                motivation = r.Motivation,
                alteredFileName = r.AlteredFileName,
                requestDate = r.RequestDate.ToString("yyyy/MM/dd HH:mm"),
                requestState = r.RequestState,
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                requests = requests.Where(r => r.username.ToLower().Contains(searchValue));
            }

            int totalRecords = requests.Count();

            IEnumerable<Object> pagedData = requests.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = requests.Count(),
                data = pagedData
            };
            return Json(response);
        }
        /// <summary>
        /// Retorna um json com a listagem de logs de pedidos aprovados relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        [Authorize(Roles = "Admin, Mod")]
        public async Task<IActionResult> GetRequestApproved(int draw, int start, int length, string searchValue)
        {
            var requestData = await _adminService.GetRequests();

            var requests = requestData.Where(r => r.RequestState == RequestState.Approved).Select(r => new
            {
                id = r.Id,
                userId = r.UserId,
                username = _userService.GetUser(r.UserId.Value).Result.Name,
                email = _userService.GetUser(r.UserId.Value).Result.Email,
                motivation = r.Motivation,
                alteredFileName = r.AlteredFileName,
                requestDate = r.RequestDate.ToString("yyyy/MM/dd HH:mm"),
                requestState = r.RequestState,
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                requests = requests.Where(r => r.username.ToLower().Contains(searchValue));
            }

            int totalRecords = requests.Count();

            IEnumerable<Object> pagedData = requests.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = requests.Count(),
                data = pagedData
            };
            return Json(response);
        }
        /// <summary>
        /// Retorna um json com a listagem de logs de pedidos pendentes relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        public async Task<IActionResult> GetRequestPending(int draw, int start, int length, string searchValue)
        {
            var requestData = await _adminService.GetRequests();

            var requests = requestData.Where(r => r.RequestState == RequestState.Pending).Select(r => new
            {
                id = r.Id,
                userId = r.UserId,
                username = _userService.GetUser(r.UserId.Value).Result.Name,
                email = _userService.GetUser(r.UserId.Value).Result.Email,
                motivation = r.Motivation,
                alteredFileName = r.AlteredFileName,
                requestDate = r.RequestDate.ToString("yyyy/MM/dd HH:mm"),
                requestState = r.RequestState,
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                requests = requests.Where(r => r.username.ToLower().Contains(searchValue));
            }

            int totalRecords = requests.Count();

            IEnumerable<Object> pagedData = requests.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = requests.Count(),
                data = pagedData
            };
            return Json(response);
        }
        /// <summary>
        /// Retorna um json com a listagem de logs de pedidos rejeitados relativamente à página solicitada.
        /// </summary>
        /// <param name="draw">Número do update</param>
        /// <param name="start">Índice de início dos dados pedidos</param>
        /// <param name="length">Quantidade de dados a enviar a partir do índice start fornecido.</param>
        /// <param name="searchValue">Valor para a pesquisa nos dados.</param>
        /// <param name="sortColumn">Coluna a ser ordenada</param>
        /// <param name="sortDirection">Ordenação ascendente ou descendente da coluna</param>
        /// <returns>Objeto JSON com os dados solicitados e filtrados.</returns>
        public async Task<IActionResult> GetRequestRejected(int draw, int start, int length, string searchValue)
        {
            var requestData = await _adminService.GetRequests();

            var requests = requestData.Where(r => r.RequestState == RequestState.Rejected).Select(r => new
            {
                id = r.Id,
                userId = r.UserId,
                username = _userService.GetUser(r.UserId.Value).Result.Name,
                email = _userService.GetUser(r.UserId.Value).Result.Email,
                motivation = r.Motivation,
                alteredFileName = r.AlteredFileName,
                requestDate = r.RequestDate.ToString("yyyy/MM/dd HH:mm"),
                requestState = r.RequestState,
            });

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();
                requests = requests.Where(r => r.username.ToLower().Contains(searchValue));
            }

            int totalRecords = requests.Count();

            IEnumerable<Object> pagedData = requests.Skip(start).Take(length);

            var response = new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = requests.Count(),
                data = pagedData
            };
            return Json(response);
        }

        /// <summary>
        /// Ação para obter o utilizador do pedido
        /// </summary>
        /// <param name="id">Variavel com o id do pedido</param>
        /// <returns>Task<User> - Obtem o utilizador com o pedido correspondente</returns
        private async Task<User> GetUserRequested(Guid id)
        {
            var request = await _adminService.GetRequestById(id);
            var userId = request.UserId.Value;
            User user = await _userService.GetUser(userId);
            return user;
        }
    }
 }
