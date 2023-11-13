using System.ComponentModel.DataAnnotations;

namespace Workio.Models.Admin.Logs
{
    /// <summary>
    /// Representa um log de um evento que possa acontecer e que não tenha sido necessáriamente desencadeado por um utilizador ou referente a um outro.
    /// </summary>
    public class SystemLog : Log
    {
        /// <summary>
        /// Texto com a descrição/Justificação do porque dá ação ter sido tomada.
        /// </summary>
        [DataType(DataType.Text)]
        [MaxLength(128)]
        public string? Description { get; set; }
    }
}
