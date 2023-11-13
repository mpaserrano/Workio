

using Workio.Models;

namespace Workio.ViewModels
{
    /// <summary>
    /// Modelo do utilizador para a view
    /// </summary>
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set;  }
        public string Name { get; set; }
        public string? ProfilePicture { get; set; } = "default.png";
        public string? AboutMe { get; set; }
        public string? GitHubAcc { get; set; }
        public string? LinkedInAcc { get; set; }
        public virtual ICollection<SkillModel> Skills { get; set; }
        public virtual ICollection<ExperienceModel> Experiences { get; set; }
        public bool OpenDMs { get; set; }
    }
}
