using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Org.BouncyCastle.Asn1.Mozilla;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workio.Models.Chat;
using Workio.Models.Events;

namespace Workio.Models
{
    public class User : IdentityUser
    {
        public User() : base()
        {
            Preferences = new UserPreferences();
        }

        [ProtectedPersonalData]
        [MaxLength(64)]
        public string Name { get; set; }
        [ProtectedPersonalData]
        public string? ProfilePicture { get; set; } = "default.png";

        [ProtectedPersonalData]
        [MaxLength(128)]
        public string? AboutMe { get; set; }
        [ProtectedPersonalData]
        public string? GitHubAcc { get; set; }
        [ProtectedPersonalData]
        public string? LinkedInAcc { get; set; }
        public Guid LanguageId { get; set; }
        [ProtectedPersonalData]
        public Localization Language { get; set; }

        /// <summary>
        /// Data e hora do registo na plataforma
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime RegisterAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Lista de habilidades, cada habilidade tem uma lista de pessoas que recomenda a habilidade deste utilizador.
        /// </summary>
        public virtual ICollection<SkillModel> Skills { get; set; } = new List<SkillModel>();
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<TeamInviteUser> TeamsRequests { get; set; } = new List<TeamInviteUser>();
        public virtual ICollection<ExperienceModel> Experiences { get; set; } = new List<ExperienceModel>();

        public virtual ICollection<BlockedUsersModel> BlockedUsers { get; set; } = new List<BlockedUsersModel>();
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        
        /// <summary>
        /// Lista de chats do utilizador
        /// </summary>
        public virtual ICollection<UserChatRoom> ChatRooms { get; set; } = new List<UserChatRoom>();

        public virtual UserPreferences Preferences { get; set; }
    }
}