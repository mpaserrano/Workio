using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    public class Tag
    {
        [Key]
        public Guid TagId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [MaxLength(24)]
        public string TagName { get; set; }
        [ForeignKey("TeamId")]
        public Guid TeamId { get; set; }
    }
}
