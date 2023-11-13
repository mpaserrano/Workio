using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    /// <summary>
    /// Classe que representa um denuncia de utilizador, extende classe de denuncias
    /// </summary>
    public class ReportUser : Report
    {
        /// <summary>
        /// String que representa o id do utilizador reportado
        /// </summary>
        public string? ReportedId { get; set; }
        /// <summary>
        /// Objeto que giarda o utilizador reportado
        /// </summary>
        [ForeignKey("ReportedId")]
        public User? ReportedUser { get; set; }

    }
}
