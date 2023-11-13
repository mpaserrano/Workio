using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models.Admin.Logs
{
    /// <summary>
    /// Representa o log das ações da administração que não influenciam uma segunda entidade.
    /// Ao seja apenas tem quem realizou a ação mas não há uma 2ª entidade que possa ser referenciada.
    /// (Ex. Alterar uma configuração qualquer)
    /// </summary>
    public class AdministrationActionLog : ActionLog
    {
        /// <summary>
        /// Tipo de ação que foi aplicada.
        /// </summary>
        public AdministrationActionLogType? ActionLogType { get; set; }
    }

    /// <summary>
    /// Representa as possíveis decisões que podem ser tomadas sobre um utilizador referente à administração.
    /// </summary>
    public enum AdministrationActionLogType
    {
        /// <summary>
        /// Tipo de ação não categorizada.
        /// </summary>
        Other = 1
    }
}
