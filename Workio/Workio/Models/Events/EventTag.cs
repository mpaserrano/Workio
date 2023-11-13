using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Workio.Models.Events
{
    /// <summary>
    /// Representa um identificador de um evento
    /// </summary>
    public class EventTag
    {
        /// <summary>
        /// Id da tag de evento
        /// </summary>
        [Key]
        public Guid EventTagId { get; set; }

        /// <summary>
        /// Nome da tag/identificador de evento
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(24)]
        public string EventTagName { get; set; }

        /// <summary>
        /// Id do evento a que faz referencia
        /// </summary>
        [ForeignKey("EventId")]
        public Guid EventId { get; set; }
    }
}
