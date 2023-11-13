using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Policy;
using Workio.Hubs;
using Workio.Migrations;
using Workio.Models;
using Workio.Services;
using Workio.Services.Email.Interfaces;
using Workio.Services.Interfaces;
using Workio.Services.Notifications;

namespace Workio.Managers.Notifications
{
    /// <summary>
    /// Gestor de notificações.
    /// Usar este gestor para enviar as notificações aos utilizadores da aplicação.
    /// </summary>
    public class NotificationManager : INotificationManager
    {
        /// <summary>
        /// Hub de notificações para fazer a comunicaão com o client em real-time
        /// </summary>
        private readonly IHubContext<NotificationHub> _notificationHub;
        /// <summary>
        /// Serviço para guardar as notificações
        /// </summary>
        private readonly INotificationService _notificationService;
        /// <summary>
        /// Serviço para enviar emails
        /// </summary>
        private readonly IEmailService _emailService;
        /// <summary>
        /// Serviço para obter dados dos utilizadores
        /// </summary>
        private readonly IUserService _userService;
        /// <summary>
        /// Serviço das traduções
        /// </summary>
        private readonly CommonLocalizationService _commonLocalizationService;

        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="notificationHub">Centro de notificações</param>
        /// <param name="notificationService">Serviço das notificações</param>
        public NotificationManager(IHubContext<NotificationHub> notificationHub, 
            INotificationService notificationService, IEmailService emailService,
            IUserService userService, CommonLocalizationService commonLocalizationService)
        {
            _notificationHub = notificationHub;
            _notificationService = notificationService;
            _emailService = emailService;
            _userService = userService;
            _commonLocalizationService = commonLocalizationService;
        }

        /// <summary>
        /// Envia uma notificação ao utilizador em real-time e guarda a mesma na base de dados
        /// </summary>
        /// <param name="userId">Id do utilizador que vai receber a notificação</param>
        /// <param name="notification">Notificação a enviar</param>
        /// <returns>true se tudo correu bem durante o envio, false caso contrário</returns>
        public async Task<bool> SendNotification(string userId, Notification notification)
        {

            try
            {
                var userPreferences = await _userService.GetUserPreferences(userId);

                if(userPreferences != null && userPreferences.ReceiveIRLNotifications == true)
                    SendRealTimeNotification(userId, notification);

                if(userPreferences != null && userPreferences.SendEmailNotifications == true)
                    SendEmailNotification(notification);

                var success = await SaveNotification(notification);
                return success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Envia uma notificação para o utilizador que recebe um novo pedido de conexão
        /// </summary>
        /// <param name="userId">Id do utilizador target</param>
        /// <param name="baseUrl">Base do url (dominio)</param>
        /// <returns>true se tudo correu bem, false caso contrário</returns>
        public async Task<bool> SendNewConnectionNotification(string userId,  string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl) || baseUrl == Guid.Empty.ToString()) return false;

            var userTarget = await _userService.GetUser(Guid.Parse(userId));

            if(userTarget == null) return false;

            string link = baseUrl + "/User/Connections/";

            Notification noti = new Notification()
            {
                Id = Guid.NewGuid(),
                Text = _commonLocalizationService.GetLocalizedString("NewConnectionNotification", userTarget.Language.Code),
                URL = link,
                UserId = userTarget.Id,
                User = userTarget
            };

            return await SendNotification(userTarget.Id, noti);
        }

        /// <summary>
        /// Envia a notificação para o client usando o hub das notificações
        /// </summary>
        /// <param name="userId">Id do utilizador destinatário da notificação</param>
        /// <param name="noti">Notificação a enviar</param>
        private void SendRealTimeNotification(string userId, Notification noti)
        {
            var notification = new
            {
                text = noti.Text,
                url = noti.URL,
            };
            Task.Run(() => _notificationHub.Clients.User(userId).SendAsync("ReceiveNotification", notification));
        }

        /// <summary>
        /// Envia a notificação por email para o utilizador
        /// </summary>
        /// <param name="noti">Notificação a enviar</param>
        private void SendEmailNotification(Notification noti)
        {
            Task.Run(() => _emailService.SendNotificationEmail(noti));
        }

        /// <summary>
        /// Guarda a notificação
        /// </summary>
        /// <param name="noti">Notificação a guardar</param>
        /// <returns>true se guardou com sucesso, false caso contrário</returns>
        private async Task<bool> SaveNotification(Notification noti)
        {
            return await _notificationService.CreateNotification(noti);
        }
    }
}
