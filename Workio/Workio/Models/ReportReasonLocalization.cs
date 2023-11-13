
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    /// <summary>
    /// Traduções das razões de denúncia
    /// </summary>
    public class ReportReasonLocalization
    {
        /// <summary>
        /// Id da tradução
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// Id da razão
        /// </summary>
        [Required]
        public Guid ReportId { get; set; }
        /// <summary>
        /// Objeto da razão
        /// </summary>
        [ForeignKey(nameof(ReportId))]
        public ReportReason ReportReason { get; set; }
        /// <summary>
        /// Código da localização (i.e. pt, en, etc)
        /// </summary>
        [Required]
        public string LocalizationCode { get; set; }
        /// <summary>
        /// Tradução
        /// </summary>
        public string Description { get; set; }
    }
}
