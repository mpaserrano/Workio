using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    public class Position
    {
        [Key]
        public Guid PositionId { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(24)]
        public string Name { get; set; }
        [ForeignKey("TeamId")]
        public Guid TeamId { get; set; }
    }
}
