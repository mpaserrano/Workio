using Workio.Models;

namespace Workio.Managers.Notifications
{
    /// <summary>
    /// Interface do gestor de notificações
    /// </summary>
    public interface INotificationManager
    {
        public Task<bool> SendNotification(string userId, Notification notification);
        public Task<bool> SendNewConnectionNotification(string userId, string baseUrl);
    }
}
