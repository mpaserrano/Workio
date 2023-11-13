using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    public class Team
    {
        [Key]
        public Guid TeamId { get; set; }

        [ForeignKey("Id")]
        public Guid? OwnerId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [MaxLength(64)]
        public string TeamName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [MaxLength(124)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public TeamStatus Status { get; set; } = TeamStatus.Open;

        public bool IsBanned { get; set; } = false;

        public Guid LanguageLocalizationId { get; set; }
        [ForeignKey(nameof(LanguageLocalizationId))]
        public Localization Language { get; set; }
        [NotMapped]
        [MaxLength(200)]
        public string Tags { get; set; }
        [NotMapped]
        [MaxLength(200)]
        public string PositionsString { get; set; }

        public virtual ICollection<Tag> Skills { get; set; } = new List<Tag>();
        public virtual ICollection<User> Members { get; set; } = new List<User>();
        public virtual ICollection<PendingUserTeam> PendingList { get; set; } = new List<PendingUserTeam>();
        public virtual ICollection<TeamInviteUser> InvitedUsers { get; set; } = new List<TeamInviteUser>();
        public virtual ICollection<Milestone>? Milestones { get; set; } = new List<Milestone>(); 
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
    }

    public enum TeamStatus
    {
        Open, Closed, Finish
    }

    public class PendingUserTeam
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid TeamId { get; set; }
        public PendingUserTeamStatus Status { get; set; } = PendingUserTeamStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }
    }

    public class TeamInviteUser
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid TeamId { get; set; }
        public PendingUserTeamStatus Status { get; set; } = PendingUserTeamStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("TeamId")]
        public Team Team { get; set; }
    }

    public enum PendingUserTeamStatus
    {
        Accepted, Pending, Rejected
    }
}
