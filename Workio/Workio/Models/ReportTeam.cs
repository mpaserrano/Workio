using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    /// <summary>
    /// Classe que representa um denuncia de equipa, extende classe de denuncias
    /// </summary>
    public class ReportTeam : Report
    {
        /// <summary>
        /// Guid que representa o id da equipa reportada
        /// </summary>
        public Guid? ReportedTeamId { get; set; }
        /// <summary>
        /// Objeto que guarda a equipa que foi reportada
        /// </summary>
        [ForeignKey("ReportedTeamId")]
        public virtual Team? ReportedTeam { get; set; }

    }
}
