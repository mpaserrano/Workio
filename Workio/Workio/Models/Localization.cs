using System.ComponentModel.DataAnnotations;

namespace Workio.Models
{
    /// <summary>
    /// Modelo de idiomas
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// Id do idioma
        /// </summary>
        [Key]
        public Guid LocalizationId { get; set; }
        /// <summary>
        /// Nome do idiomna
        /// </summary>
        [Required]
        public string Language { get; set; }
        /// <summary>
        /// Nome do icon do idioma
        /// </summary>
        public string IconName { get; set; }
        /// <summary>
        /// Código do idioma
        /// I.E.: Portugal - pt; English - en
        /// </summary>
        public string Code { get; set; }
    }
}
