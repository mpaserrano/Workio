using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;

namespace Workio.Services.Notifications
{
    /// <summary>
    /// Implementação da interface do serviço das notificações
    /// Este serviço serve para manipular as notificações e como sistema de crud para a tabela das notificações na base de dados
    /// </summary>
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// Contexto da base de dados
        /// </summary>
        private ApplicationDbContext _context;

        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="context">Contexto da base de dados a ser utilizada pelo serviço</param>
        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtem uma notificação com um determinado id
        /// </summary>
        /// <param name="id">Id da notificação</param>
        /// <returns>Objeto da notificação</returns>
        /// <exception cref="ArgumentNullException">Quando id é nulo é lançada uma exceção</exception>
        /// <exception cref="ArgumentException">Quando não existe um record com o id do parametro é lançada uma exceção</exception>
        public async Task<Notification> GetNotificationAsync(Guid id)
        {
            if (Guid.Empty == id) throw new ArgumentNullException(nameof(id));
            var noti = await _context.Notifications.Include(x => x.User).Where(x => x.Id == id).FirstOrDefaultAsync();

            if (noti == null) throw new ArgumentException();

            return noti;
        }
        /// <summary>
        /// Guarda a notificação na base de dados configurada
        /// </summary>
        /// <param name="notification">Notificação a ser guardada</param>
        /// <returns>true se guardou com sucesso, false caso contrário</returns>
        /// <exception cref="ArgumentNullException">Quando a notificação é nula ou está vazia é lançada uma exceção</exception>
        public async Task<bool> CreateNotification(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            if (string.IsNullOrEmpty(notification.Text) || Guid.Empty == Guid.Parse(notification.UserId)) throw new ArgumentNullException(nameof(notification));
            var success = 0;
            try
            {
                notification.IsRead = false;
                _context.Notifications.Add(notification);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                success = 0;
            }

            return success == 1;
        }

        /// <summary>
        /// Marca uma notificação como lida
        /// </summary>
        /// <param name="notification">Notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        /// <exception cref="ArgumentNullException">Quando a notificação é nula é lançada uma exceção</exception>
        public async Task<bool> MarkAsRead(Notification notification)
        {
            if(notification == null) throw new ArgumentNullException(nameof(notification));
            if (!(await NotificationExist(notification.Id))) return false;
            var success = 0;
            try
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                success = 0;
            }

            return success == 1;
        }
        /// <summary>
        /// Marca uma notificação como não lida
        /// </summary>
        /// <param name="notification">Notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        /// <exception cref="ArgumentNullException">Quando a notificação é nula é lançada uma exceção</exception>
        public async Task<bool> MarkAsUnread(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            if (!(await NotificationExist(notification.Id))) return false;
            var success = 0;
            try
            {
                notification.IsRead = false;
                _context.Notifications.Update(notification);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                success = 0;
            }

            return success == 1;
        }
        /// <summary>
        /// Marca uma notificação como lida
        /// </summary>
        /// <param name="id">Id da notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        /// <exception cref="ArgumentNullException">Quando o id está vazio ou é nulo é lançada uma exceção</exception>
        public async Task<bool> MarkAsRead(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException("Empty id");
            var noti = await GetNotificationAsync(id);
            if(noti == null) return false;
            var success = 0;
            try
            {
                noti.IsRead = true;
                _context.Notifications.Update(noti);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                success = 0;
            }

            return success == 1;
        }
        /// <summary>
        /// Marca uma notificação como não lida
        /// </summary>
        /// <param name="id">Id da notificação a alterar</param>
        /// <returns>true caso a notificação tenha sido atualizada com sucesso, false caso contrário</returns>
        /// <exception cref="ArgumentNullException">Quando o id está vazio ou é nulo é lançada uma exceção</exception>
        public async Task<bool> MarkAsUnread(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException("Empty id");
            var noti = await GetNotificationAsync(id);
            if (noti == null) return false;
            var success = 0;
            try
            {
                noti.IsRead = false;
                _context.Notifications.Update(noti);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                success = 0;
            }

            return success == 1;
        }

        /// <summary>
        /// Verifica se uma notificação com um determinado id existe
        /// </summary>
        /// <param name="id">Id da notificação</param>
        /// <returns>true se a notificação existe na DB, false caso contrário</returns>
        /// <exception cref="ArgumentNullException">Lança uma exceção caso o id seja nulo ou vazio</exception>
        private async Task<bool> NotificationExist(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentNullException("Id empty");
            var noti = await _context.Notifications.AnyAsync(x => x.Id == id);
            return noti;
        }
    }
}
