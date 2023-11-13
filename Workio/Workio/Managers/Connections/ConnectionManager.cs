namespace Workio.Managers.Connections
{
    public class ConnectionManager : IConnectionManager
    {
        //Using hashset to not have duplicated connections
        private static Dictionary<string, HashSet<string>> userMap = new Dictionary<string, HashSet<string>>();

        //Get users online connect to the hub
        public IEnumerable<string> OnlineUsers { get { return userMap.Keys; }}
    
        public void AddConnection(string userId, string connectionId)
        {
            lock(userMap)
            {
                if (!userMap.ContainsKey(userId))
                {
                    userMap[userId] = new HashSet<string>();
                }
                userMap[userId].Add(connectionId);
            }
        }

        public void RemoveConnection(string connectionId) 
        {
            lock (userMap)
            {
                foreach(var username in userMap.Keys)
                {
                    if(userMap.ContainsKey(username))
                    {
                        if (userMap[username].Contains(connectionId))
                        {
                            userMap[username].Remove(connectionId);
                            break;
                        }
                    }
                }
            }
        }

        public HashSet<string> GetConnections(string userId)
        {
            var conn = new HashSet<string>();
            try
            {
                lock (userMap)
                {
                    conn = userMap[userId];
                }
            }
            catch
            {
                conn = null;
            }
            return conn;
        }
    }
}
