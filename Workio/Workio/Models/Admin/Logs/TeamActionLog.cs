using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models.Admin.Logs
{
    /// <summary>
    /// Representa uma ação sobre uma equipa por parte um utilizador.
    /// (Ex: No caso de uma equipa não se encontrar nos padrões pode ser removida)
    /// </summary>
    public class TeamActionLog : ActionLog
    {
        /// <summary>
        /// Id da equipa da qual o seu estado foi modificado.
        /// </summary>
        public Guid? ChangedTeamId { get; set; }

        /// <summary>
        /// Objeto da equipa da qual o seu estado foi modificado.
        /// </summary>
        [ForeignKey("ChangedTeamId")]
        public Team? ChangedTeam { get; set; }

        /// <summary>
        /// Tipo de ação tomada.
        /// </summary>
        public TeamActionLogType? ActionLogType { get; set; }
    }

    /// <summary>
    /// Representa a representação de possíbilidade de tipo de ação de log
    /// que foi atribúido.
    /// </summary>
    public enum TeamActionLogType
    {
        /// <summary>
        /// Tipo de ação desconhecida.
        /// </summary>
        Other = 1,
        /// <summary>
        /// Ação de banir uma equipa.
        /// </summary>
        Ban = 2,
        /// <summary>
        /// Ação de remover o ban de uma equipa.
        /// </summary>
        Unban = 3,
        /// <summary>
        /// Ação de ocultar um evento.
        /// </summary>
        Hidden = 100
    }
}
