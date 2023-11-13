using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    public class RatingModel
    {
        [Key]
        public Guid RatingId { get; set; }

        [ForeignKey("Id")]
        public Guid? ReceiverId { get; set; }
        [Range(0, 5, ErrorMessage = "Value for {0} must be between {1} and {2}."), Required, Display(Name = "Rating")]
        public int Rating { get; set; }
        [ForeignKey("Id")]
        public Guid? RaterId { get; set; }
        [Display(Name = "Why did you give that rating? (Optional)")]
        [MaxLength(64)]
        public string? Comment { get; set; }
    }
}