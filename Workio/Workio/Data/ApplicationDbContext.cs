using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using Workio.Models;
using Workio.Models.Events;
using Workio.Models.Admin.Logs;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using Workio.Models.Chat;

namespace Workio.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
        public DbSet<Workio.Models.SkillModel> SkillModel { get; set; }
        public DbSet<ExperienceModel> ExperienceModel { get; set; }
        public DbSet<Workio.Models.RequestEntityStatus> RequestEntityStatus { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Localization> Localizations { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<PendingUserTeam> PendingUsers { get; set; }
        public DbSet<ReportUser> ReportUser { get; set; }
        public DbSet<ReportTeam> ReportTeams { get; set; }
        public DbSet<ReportEvent> ReportEvents { get; set; }
        public DbSet<Workio.Models.ReportReason> ReportReason { get; set; }
        public DbSet<InterestedUser> InterestedUsers { get; set; }
        public DbSet<InterestedTeam> InterestedTeams { get; set; }
        public DbSet<TeamInviteUser> TeamsRequests { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BlockedUsersModel>()
                .HasOne(l => l.SourceUser)
                .WithMany(a => a.BlockedUsers)
                .HasForeignKey(l => l.SourceUserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<User>()
                .HasOne(l => l.Language)
                .WithMany()
                .HasForeignKey(l => l.LanguageId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<UserChatRoom>()
                .HasKey(m => new { m.ChatRoomId, m.UserId});
            modelBuilder.Entity<Connection>()
                .HasOne(c => c.RequestOwner)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Connection>()
                .HasOne(c => c.RequestedUser)
                .WithMany()
                .HasForeignKey(c => c.RequestedUserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<InterestedUser>()
                .HasOne(c => c.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Workio.Models.BlockedUsersModel> BlockedUsersModel { get; set; }

        public DbSet<Workio.Models.Team> Team { get; set; }

        public DbSet<Workio.Models.RatingModel> RatingModel { get; set; }

        public DbSet<Workio.Models.Events.Event> Event { get; set; }

        public DbSet<Workio.Models.Events.EventReactions> EventReactions { get; set; }

        public DbSet<Workio.Models.Events.EventTag> EventTag { get; set; }

        public DbSet<Workio.Models.Admin.Logs.SystemLog> SystemLog { get; set; }

        public DbSet<Workio.Models.Admin.Logs.UserActionLog> UserActionLog { get; set; }

        public DbSet<Workio.Models.Admin.Logs.AdministrationActionLog> AdministrationActionLogs { get; set; }

        public DbSet<Workio.Models.Admin.Logs.TeamActionLog> TeamActionLogs { get; set; }
        
        public DbSet<Workio.Models.Admin.Logs.EventActionLog> EventActionLogs { get; set; }

        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<UserChatRoom> UserChatRooms { get; set;}
        public DbSet<UserPreferences> UserPreferences { get; set;}
        public DbSet<ReportReasonLocalization> ReportReasonLocalizations { get; set; }
    }
}