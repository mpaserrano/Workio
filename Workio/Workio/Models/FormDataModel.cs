using System.ComponentModel.DataAnnotations;

namespace Workio.Models
{
    /// <summary>
    /// Classe que representa a informação vinda do formulário de adicionar novas razões de denuncia
    /// </summary>
    public class FormDataModel
    {
        /// <summary>
        /// Variavel que guarda o tipo de denuncia vinda do formulário de adicionar denuncias
        /// </summary>
        public string ReasonType { get; set; }
        /// <summary>
        /// Variavel que guarda a razão de denuncia vinda do formulário de adicionar denuncias
        /// </summary>
        [Required]
        public string Reason { get; set; }
        /// <summary>
        /// Tradução da razão para português
        /// </summary>
        [Required]
        public string ReasonPortugues { get; set; }
        /// <summary>
        /// Variavel que guarda a justificação para o adicionar da razão de report que entra nas logs
        /// </summary>
        [Required]
        public string Description { get; set; }
    }
}
