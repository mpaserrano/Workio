using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models.Chat
{
    /// <summary>
    /// Objeto que contem informação sobre os leitores de uma mensagem
    /// </summary>
    public class MessageReader
    {
        /// <summary>
        /// Id da leitura
        /// </summary>
        [Key]
        public Guid MessageReaderId { get; set; }

        /// <summary>
        /// Id do utilizador que leu a mensagem
        /// </summary>
        public string ReaderId { get; set; }

        /// <summary>
        /// Data e hora, em utc, de quando a mensagem foi lida
        /// </summary>
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Objeto do utilizador que leu a mensagem
        /// </summary>
        [ForeignKey(nameof(ReaderId))]
        public virtual User Reader { get; set; }
    }
}
