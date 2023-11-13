using System.ComponentModel.DataAnnotations;

namespace Workio.Models
{
    public class LogViewModel
    {
        [Required]
        [MaxLength(128)]
        public string Description { get; set; }
    }
}
