using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Workio.Managers.Notifications;
using Workio.Models;
using Workio.Services.Interfaces;
using Workio.Services.Notifications;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador com ações para obter notificações e mudar o seu estado
    /// </summary>
    public class NotificationsController : Controller
    {
        /// <summary>
        /// Manager das sessões
        /// </summary>
        private readonly SignInManager<User> _signInManager;
        /// <summary>
        /// Manager dos utilizadores
        /// </summary>
        private readonly UserManager<User> _userManager;
        /// <summary>
        /// Serviço para receber informações dos users
        /// </summary>
        private IUserService _userService;
        /// <summary>
        /// Serviço das notificações
        /// </summary>
        private INotificationService _notificationService;

        /// <summary>
        /// Construtor do controlador
        /// </summary>
        /// <param name="signInManager">Manager de sessões</param>
        /// <param name="userManager">Manager do utilizador</param>
        /// <param name="userService">Serviço para interagir com a bd - oferece os dados dos utilizadores</param>
        /// <param name="notificationService">Serviço para obter os dados das notificações</param>
        public NotificationsController(SignInManager<User> signInManager, UserManager<User> userManager,
            IUserService userService, INotificationService notificationService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Ação que mostra a página das notificações
        /// </summary>
        /// <returns>Página das notificações</returns>
        public async Task<IActionResult> Index()
        {
            if (!_signInManager.IsSignedIn(User)) RedirectToAction("Index", "Home");

            
            return View();
        }

        /// <summary>
        /// Obtem as notificações do utilizador logado
        /// </summary>
        /// <param name="status">Estado para filtrar as notificações (se está ou não lidas) *opcional</param>
        /// <returns>JSON com as notificaçõesS</returns>
        public async Task<IActionResult> GetNotifications(bool? status)
        {
            if (!_signInManager.IsSignedIn(User)) return BadRequest();

            var userId = CurrentUserId();

            if(userId == Guid.Empty) return BadRequest();

            var user = await _userService.GetUser(userId);

            Console.WriteLine(status);

            var notifications = user.Notifications.Select(x => new {
                Id = x.Id,
                Text = x.Text,
                URL = x.URL,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            });

            if(status == true || status == false)
            {
                notifications = notifications.Where(x => x.IsRead == status)
                    .ToList();
            }

            notifications = notifications.OrderByDescending(x => x.CreatedAt.Date).ThenByDescending(x => x.CreatedAt.TimeOfDay);

            return Json(notifications);
        }

        /// <summary>
        /// Altera o estado de uma notificação entre lida e por ler
        /// </summary>
        /// <param name="notificationId">Id da notificação</param>
        /// <param name="status">Estado atual da notificação</param>
        /// <returns>Status 200 se correu bem, 400 caso algo tenha corrido mal ou o id seja inválido</returns>
        public async Task<IActionResult> ChangeStatus(string notificationId, bool status)
        {
            if(string.IsNullOrEmpty(notificationId) || notificationId == Guid.Empty.ToString()) return BadRequest();

            var success = false;

            if(status == false)
            {
                success = await _notificationService.MarkAsRead(new Guid(notificationId));
            }
            else
            {
                success = await _notificationService.MarkAsUnread(new Guid(notificationId));
            }
            
            return success ? Ok(success) : BadRequest();
        }

        /// <summary>
        /// Obtem o número total de notificações por ler
        /// </summary>
        /// <returns>Número de notificações por ler</returns>
        public async Task<int> GetNotificationsCount()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var userId = CurrentUserId();
                if (userId == Guid.Empty)
                {
                    return 0;
                }
                else
                {
                    var user = await _userService.GetUser(userId);
                    return user.Notifications.Where(x => !x.IsRead).Count();
                }
            }

            return 0;
        }

        private Guid CurrentUserId()
        {
            if (!_signInManager.IsSignedIn(User)) return Guid.Empty;
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
