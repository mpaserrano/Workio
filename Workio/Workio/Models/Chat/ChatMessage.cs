using System.ComponentModel.DataAnnotations;

namespace Workio.Models.Chat
{
    /// <summary>
    /// Objeto de uma mensagem do chat
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Id da mensagem
        /// </summary>
        [Key]
        public Guid MessageId { get; set; }
        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [StringLength(250)]
        public string Text { get; set; }

        /// <summary>
        /// Data e hora do envio da mensagem - este valor está de acorco com o UTC
        /// </summary>
        public DateTime SendAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Id do utilizador que enviou a mensagem
        /// </summary>
        public string SenderId { get; set; }
        /// <summary>
        /// Objeto do utilizador que enviou a mensagem
        /// </summary>
        public virtual User Sender { get; set; }

        /// <summary>
        /// Coleção com os utilizadores que leram a mensagem e a hora que a leram
        /// </summary>
        public virtual ICollection<MessageReader> Readers { get; set; }
    }
}
