using System.ComponentModel.DataAnnotations;

namespace Workio.Models.Chat
{
    /// <summary>
    /// Sala ce conversações
    /// </summary>
    public class ChatRoom
    {
        /// <summary>
        /// Id da sala
        /// </summary>
        [Key]
        public Guid ChatRoomId { get; set; }
        /// <summary>
        /// Nome da sala
        /// </summary>
        [DataType(DataType.Text)]
        [StringLength(64)]
        public string? ChatRoomName { get; set; }
        /// <summary>
        /// Lista de membros participantes na conversa
        /// </summary>
        public virtual ICollection<UserChatRoom> Members { get; set; } = new List<UserChatRoom>();
        /// <summary>
        /// Coleção de mensagens enviadas para este chat
        /// </summary>
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        /// <summary>
        /// (Opcional) Id da equipa que faz parte desta conversa
        /// Null -> Conversa user-to-user
        /// !Null -> Conversa em grupo
        /// </summary>
        public Guid? TeamId { get; set; }
        /// <summary>
        /// (Opcional) Equipa que pode fazer parte desta conversa
        /// </summary>
        public virtual Team? Team { get; set; }
    }
}
