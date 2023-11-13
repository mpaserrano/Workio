using System.ComponentModel.DataAnnotations.Schema;
using Workio.Models.Events;

namespace Workio.Models
{
    /// <summary>
    /// Classe que representa um denuncia de evento, extende classe de denuncias
    /// </summary>
    public class ReportEvent : Report
    {
        /// <summary>
        /// Id do evento denunciado
        /// </summary>
        public Guid? ReportedEventId { get; set; }
        /// <summary>
        /// Objeto que guarda o evento que foi denunciado
        /// </summary>
        [ForeignKey("ReportedEventId")]
        public virtual Event? ReportedEvent { get; set; }

    }
}
