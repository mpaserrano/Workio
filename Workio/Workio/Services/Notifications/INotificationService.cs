using Workio.Models;

namespace Workio.Services.Notifications
{
    /// <summary>
    /// Interface do serviço de notificações (crud na base de dados)
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Obtem uma notificação com um determinado id
        /// </summary>
        /// <param name="id">Id da notificação</param>
        /// <returns>Objeto da notificação</returns>
        public Task<Notification> GetNotificationAsync(Guid id);
        /// <summary>
        /// Guarda a notificação na base de dados configurada
        /// </summary>
        /// <param name="notification">Notificação a ser guardada</param>
        /// <returns>true se guardou com sucesso, false caso contrário</returns>
        public Task<bool> CreateNotification(Notification notification);
        /// <summary>
        /// Marca uma notificação como lida
        /// </summary>
        /// <param name="notification">Notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        public Task<bool> MarkAsRead(Notification notification);
        /// <summary>
        /// Marca uma notificação como não lida
        /// </summary>
        /// <param name="notification">Notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        public Task<bool> MarkAsUnread(Notification notification);
        /// <summary>
        /// Marca uma notificação como lida
        /// </summary>
        /// <param name="id">Id da notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        public Task<bool> MarkAsRead(Guid id);
        /// <summary>
        /// Marca uma notificação como não lida
        /// </summary>
        /// <param name="id">Id da notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        public Task<bool> MarkAsUnread(Guid id);
    }
}
