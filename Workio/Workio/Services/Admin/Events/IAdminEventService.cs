using Workio.Data;
using Workio.Models;
using Workio.Models.Events;

namespace Workio.Services.Admin.Events
{
    /// <summary>
    /// Interface para o administrador interagir com os eventos a partir da lista de eventos disponivel no dashboard
    /// </summary>
    public interface IAdminEventService
    {


        /// <summary>
        /// Obtem todas os eventos.
        /// </summary>
        /// <returns>Collection de eventos</returns>
        public Task<ICollection<Event>> GetEvents();

        /// <summary>
        /// Bane um evento.
        /// </summary>
        /// <returns>sucesso se baniu um evento</returns>
        public Task<bool> BanEvent(Guid eventId);

        /// <summary>
        /// Remove o ban um evento.
        /// </summary>
        /// <returns>sucesso se removeu o ban um evento</returns>
        public Task<bool> UnbanEvent(Guid eventId);

        /// <summary>
        /// Mete um evento como featured
        /// </summary>
        /// <param name="eventId">Id do evento</param>
        /// <returns>true se foi alterado com sucesso, false caso contrário</returns>
        public Task<bool> MarkAsFeatured(Guid eventId);

        /// <summary>
        /// Remove um evento de featured
        /// </summary>
        /// <param name="eventId">Id do evento</param>
        /// <returns>true se foi alterado com sucesso, false caso contrário</returns>
        public Task<bool> RemoveFeatured(Guid eventId);

    }
}
