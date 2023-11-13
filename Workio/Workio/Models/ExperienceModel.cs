using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workio.Attributes;

namespace Workio.Models
{
    /// <summary>
    /// Modelo representativo da experiência de trabalho de um utilizador
    /// </summary>
    public class ExperienceModel
    {
        [Key]
        public Guid ExperienceId { get; set; }
        [ValidateNever]
        public string UserId { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(64)]
        public string WorkTitle { get; set; }
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(32)]
        public string Company { get; set; }
        [DataType(DataType.MultilineText)]
        [MaxLength(100)]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [CurrentDate(ErrorMessage = "Max date must be current date")]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Date)]
        [IsDateAfterAttribute("StartDate", allowEqualDates: true)]
        public DateTime? EndDate { get; set; }
    }
}
