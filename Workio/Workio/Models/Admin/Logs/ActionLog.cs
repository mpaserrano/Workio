using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models.Admin.Logs
{
    /// <summary>
    /// Faz um registo em log relativamente a ações que são desencadeadas por utilizadores.
    /// Ex: Banir um utilizador, encerrar uma equipa, dar mode de administrador a um utilizador... São todas as
    /// ações que envolvem interação por parte de um utilizador.
    /// </summary>
    public class ActionLog : Log
    {
        /// <summary>
        /// Id do utilizador responsável pelo aparecimento do log.
        /// Ao seja o Id utilizador que realizou a ação.
        /// </summary>
        public string? AuthorId { get; set; }
        /// <summary>
        /// Utilizador responsável pelo aparecimento do log.
        /// O utilizador que realizou a ação.
        /// </summary>
        [ForeignKey("AuthorId")]
        public virtual User? AuthorUser { get; set; }
        /// <summary>
        /// Texto com a descrição/Justificação do porque dá ação ter sido tomada.
        /// </summary>
        [DataType(DataType.Text)]
        [MaxLength(128)]
        public string? ActionDescription { get; set; }
    }
}
