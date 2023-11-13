using Workio.Models.Events;
using System.ComponentModel.DataAnnotations;

namespace Workio.Models
{
    public class InterestedTeam
    {
        /// <summary>
        /// Id do objeto utilizador interessad
        /// </summary>
        [Key]
        public Guid InterestedId { get; set; }
        /// <summary>
        /// Objeto correspondente ao utilizador que mostrou interesse
        /// </summary>
        public Team Team { get; set; }
        /// <summary>
        /// Objeto que corresponde ao evento
        /// </summary>
        public Event Event { get; set; }
    }
}
