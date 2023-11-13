using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    public class Milestone
    {
        [Key]
        public Guid MilestoneId { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(24)]
        public string Name { get; set; }
        [DataType(DataType.Text)]
        [MaxLength(100)]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [CheckDateRange]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [IsDateAfterAttribute("StartDate", allowEqualDates: true)]
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
        [Required]
        public MilestoneState State { get; set; } = MilestoneState.Active;
        [ForeignKey("TeamId")]
        public Guid TeamId { get; set; }
    }

    public enum MilestoneState
    {
        Active, Completed
    }
}
