using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace Workio.Models
{
    /// <summary>
    /// Classe que representa uma conexão feita por 2 utilizadores.
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Id da conexão
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// Id do utilizador que enviou o pedido de conexão
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Id do utilizador que recebeu o pedido de conexão
        /// </summary>
        public string RequestedUserId { get; set; }
        /// <summary>
        /// Estado da conexão
        /// </summary>
        public ConnectionState State { get; set; }
        /// <summary>
        /// Data que o pedido da conexão foi feita
        /// </summary>
        public DateTime ConnectionDate { get; set; }
        /// <summary>
        /// Utilizador que enviou o pedido de conexão
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User RequestOwner { get; set; }
        /// <summary>
        /// Utilizador que recebeu o pedido de conexão
        /// </summary>
        [ForeignKey(nameof(RequestedUserId))]
        public User RequestedUser { get; set; }
    }
}
