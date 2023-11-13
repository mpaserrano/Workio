using System.ComponentModel.DataAnnotations;
using Workio.Models.Events;

namespace Workio.Models
{
    public class InterestedUser
    {
        /// <summary>
        /// Id do objeto utilizador interessad
        /// </summary>
        [Key]
        public Guid InterestedId { get; set; }
        /// <summary>
        /// Objeto correspondente ao utilizador que mostrou interesse
        /// </summary>
        public User User { get; set; }
        /// <summary>
        /// Objeto que corresponde ao evento
        /// </summary>
        public Event Event { get; set; }
    }
}
