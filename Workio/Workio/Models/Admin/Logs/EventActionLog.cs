using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workio.Models.Events;

namespace Workio.Models.Admin.Logs
{
    /// <summary>
    /// Representa o log das ações dos eventos.
    /// </summary>
    public class EventActionLog : ActionLog
    {
        /// <summary>
        /// Id do evento do qual o seu estado foi modificado.
        /// </summary>
        public Guid? ChangedEventId { get; set; }

        /// <summary>
        /// Objeto do evento do qual o seu estado foi modificado.
        /// </summary>
        [ForeignKey("ChangedEventId")]
        public Event? ChangedEvent { get; set; }

        /// <summary>
        /// Tipo de ação tomada.
        /// </summary>
        public EventActionLogType? ActionLogType { get; set; }
    }

    /// <summary>
    /// Representa as possíveis categorias de tipo de logs que podem
    /// ser atribuídas ao evento.
    /// </summary>
    public enum EventActionLogType
    {
        /// <summary>
        /// Tipo de ação desconhecida.
        /// </summary>
        Other = 1,
        /// <summary>
        /// Ação de banir um evento.
        /// </summary>
        Ban = 2,
        /// <summary>
        /// Ação de remover o ban de um evento.
        /// </summary>
        Unban = 3,
        /// <summary>
        /// Ação para marcar como featured
        /// </summary>
        MarkAsFeatured = 4,
        /// <summary>
        /// Ação para remover de featured
        /// </summary>
        RemoveFeatured = 5,
        /// <summary>
        /// Ação de ocultar um evento.
        /// </summary>
        Hidden = 100
    }
}
