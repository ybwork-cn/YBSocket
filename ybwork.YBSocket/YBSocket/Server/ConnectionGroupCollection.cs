using System.Collections.Generic;

namespace ybwork.YBSocket.Server
{
    public class ConnectionGroupCollection
    {
        private readonly ConnectionClientCollection Clients;
        private readonly Dictionary<string, List<string>> Groups = new();
        private readonly Dictionary<string, string> GroupMap = new();

        private readonly object locker = new object();

        public ConnectionGroupCollection(ConnectionClientCollection clients)
        {
            Clients = clients;
        }

        public ConnectionClientCollection this[string groupId]
        {
            get
            {
                if (!Groups.TryGetValue(groupId, out List<string> group))
                    return new ConnectionClientCollection(Clients);
                return new ConnectionClientCollection(Clients, group);
            }
        }

        public void SetGroup(string clientId, string groupId)
        {
            Remove(clientId);

            lock (locker)
            {
                GroupMap[clientId] = groupId;

                if (!Groups.TryGetValue(groupId, out List<string> group))
                {
                    group = new List<string>();
                    Groups.Add(groupId, group);
                }

                group.Add(clientId);
            }
        }

        public void Remove(string clientId)
        {
            lock (locker)
            {
                if (GroupMap.ContainsKey(clientId) && Groups.TryGetValue(GroupMap[clientId], out List<string> group))
                {
                    group.Remove(clientId);
                }
            }
        }
    }
}
