using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models.Chat
{
    /// <summary>
    /// Objecto de uma sala ativa por parte de um user
    /// </summary>
    public class UserChatRoom
    {
        /// <summary>
        /// Id da conversa
        /// </summary>
        public Guid ChatRoomId { get; set; }
        /// <summary>
        /// Id do user
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Objeto da sala
        /// </summary>
        [ForeignKey(nameof(ChatRoomId))]
        public ChatRoom ChatRoom { get; set; }
        /// <summary>
        /// Objeto do utilizador
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        public UserChatRoomStatus Status { get; set; }
    }

    public enum UserChatRoomStatus
    {
        Active = 1,
        Archived = 2
    }
}
