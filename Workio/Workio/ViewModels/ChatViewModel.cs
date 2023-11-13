namespace Workio.ViewModels
{
    public class ChatViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Email { get; set; }
        public ChatViewModelType Type { get; set; }
    }

    public enum ChatViewModelType
    {
        User, Team
    }
}
