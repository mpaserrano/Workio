using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models.Admin.Logs
{
    /// <summary>
    /// Representa uma ação sobre um utilizador por parte de um outro (pode ser um administrador ou outro).
    /// </summary>
    public class UserActionLog : ActionLog
    {
        /// <summary>
        /// Id do utilizador da qual foi feita a decisão de alterar ou não o seu estado.
        /// </summary>
        public string? ChangedUserId { get; set; }

        /// <summary>
        /// Objeto do utilizador do qual foi feita a decisão de alterar ou não o seu estado.
        /// </summary>
        [ForeignKey("ChangedUserId")]
        public User? ChangedUser { get; set; }

        /// <summary>
        /// Tipo de ação tomada.
        /// </summary>
        public UserActionLogType? ActionLogType { get; set; }
    }

    /// <summary>
    /// Representa as possíveis decisões que podem ser tomadas sobre um utilizador
    /// </summary>
    public enum UserActionLogType
    {
        /// <summary>
        /// Tipo de ação desconhecida.
        /// </summary>
        Other = 1,
        /// <summary>
        /// Ação de banir um utilizador.
        /// </summary>
        Banned = 2,
        /// <summary>
        /// Ação para remover o ban de um utilizador
        /// </summary>
        Unbanned = 3,
        /// <summary>
        /// Ação de dar previlégio de entidade certificada a um utilizador.
        /// </summary>
        GiveEntity = 50,
        /// <summary>
        /// Ação de dar previlégio de administrador a um utilizador.
        /// </summary>
        GivedAdmin = 100,
        /// <summary>
        /// Ação de dar um novo role a um utilizador
        /// </summary>
        GivedRole = 110,
        /// <summary>
        /// Ação de retirar um role a um utilizador
        /// </summary>
        RemovedRole = 115
    }
}
