namespace Workio.Models
{
    public class UserPreferences
    {
        public Guid UserPreferencesId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Describes if user receives messages from other users
        /// </summary>
        public bool IsDMOpen { get; set; } = true;

        /// <summary>
        /// Describes if user receives email notifications
        /// </summary>
        public bool SendEmailNotifications { get; set; } = false;

        /// <summary>
        /// Describes if user receives IRL notifications
        /// </summary>
        public bool ReceiveIRLNotifications { get; set; } = true;

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
