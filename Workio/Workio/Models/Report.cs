using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    /// <summary>
    /// Class que representa uma denuncia
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Id da denuncia
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// Id do utilizador que denunciou
        /// </summary>
        public string? ReporterId { get; set; }
        /// <summary>
        /// Objeto de User que contem o utilizador que denunciou
        /// </summary>
        [ForeignKey("ReporterId")]
        public virtual User? Reporter { get; set; }
        /// <summary>
        /// Id da razão de denuncia
        /// </summary>
        public Guid? ReportReasonId { get; set; }
        /// <summary>
        /// Objeto de ReportReason que contem a razão da denuncia
        /// </summary>
        [ForeignKey("ReportReasonId")]
        public virtual ReportReason? ReportReason { get; set; }
        /// <summary>
        /// Objeto do tipo ReportStatus que demonstra o estado da denuncia
        /// </summary>
        public ReportStatus ReportStatus { get; set; } = ReportStatus.Pending;
        /// <summary>
        /// Descrição da denuncia
        /// </summary>
        [Required]
        [MaxLength(124)]
        public string Description { get; set; }
        /// <summary>
        /// Data da denuncia
        /// </summary>
        public DateTime Date { get; set; }

    }
}
