using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class SkillModel
    {
        [Key]
        public Guid SkillId { get; set; }
        [Required]
        [Display(Name = "SkillName")]
        [MaxLength(24)]
        public string Name { get; set; }
        [ForeignKey("Id")]
        public Guid? UserId { get; set; }

        public virtual List<User>? Endorsers { get; set; } = new List<User>();
    }
}
