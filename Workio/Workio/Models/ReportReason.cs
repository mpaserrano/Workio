using System.ComponentModel.DataAnnotations;

namespace Workio.Models
{
    /// <summary>
    /// Classe que representa as possiveis razões pré definidas para denuncias
    /// </summary>
    public class ReportReason
    {
        /// <summary>
        /// Id da razão
        /// </summary>
        [Key]
        public Guid? Id { get; set; }
        /// <summary>
        /// Razão em texto
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string? Reason { get; set; }
        /// <summary>
        /// Representa o tipo de razão da denuncia, se é uma rasão para denuncia de utilizador,equipa ou evento
        /// </summary>
        public ReasonType ReasonType { get; set; }

    }
}
