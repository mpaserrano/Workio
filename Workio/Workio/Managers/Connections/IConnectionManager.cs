namespace Workio.Managers.Connections
{
    public interface IConnectionManager
    {
        void AddConnection(string userId, string connectionId);
        void RemoveConnection(string connectionId);
        HashSet<string> GetConnections(string userId);
        IEnumerable<string> OnlineUsers { get; }
    }
}
