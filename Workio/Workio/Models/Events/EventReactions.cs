using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models.Events
{
    public class EventReactions
    {
        /// <summary>
        /// Id da reação
        /// </summary>
        [Key]
        public Guid ReactionId { get; set; }

        /// <summary>
        /// Id do utilizador que reagiu
        /// </summary>
        [ForeignKey("Id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Id do evento a que o utilizador reagiu
        /// </summary>
        [ForeignKey("EventId")]
        public Guid EventId { get; set; }

        /// <summary>
        /// A reação que o utilizador deu no evento
        /// </summary>
        public EventReactionType ReactionType { get; set; }
    }

    /// <summary>
    /// Representa um voto positivo ou um negativo
    /// </summary>
    public enum EventReactionType
    {
        UpVote, DownVote
    }
}
