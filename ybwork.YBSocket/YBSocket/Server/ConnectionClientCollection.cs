using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ybwork.YBSocket.Server
{
    public class ConnectionClientCollection : IEnumerable<ConnectionClient>
    {
        private readonly Dictionary<string, ConnectionClient> Clients;
        private readonly List<string> ClientIds = new();

        public ConnectionClientCollection this[string clientId]
            => new ConnectionClientCollection(this, clientId);

        public ConnectionClientCollection this[IEnumerable<string> clientIds]
            => new ConnectionClientCollection(this, clientIds.Where(ClientIds.Contains));

        internal ConnectionClientCollection()
        {
            Clients = new();
        }

        internal ConnectionClientCollection(ConnectionClientCollection clients)
        {
            Clients = clients.Clients;
        }

        internal ConnectionClientCollection(ConnectionClientCollection clients, string clientId) : this(clients)
        {
            ClientIds.Add(clientId);
        }

        internal ConnectionClientCollection(ConnectionClientCollection clients, IEnumerable<string> clientIds) : this(clients)
        {
            ClientIds.AddRange(clientIds);
        }

        internal void Add(ConnectionClient client)
        {
            Clients[client.ClinetId] = client;
            ClientIds.Add(client.ClinetId);
        }

        internal void Remove(ConnectionClient client)
        {
            Clients.Remove(client.ClinetId);
            ClientIds.Remove(client.ClinetId);
        }

        internal ConnectionClient GetClient(string clientId)
        {
            TryGetClient(clientId, out ConnectionClient client);
            return client;
        }

        internal bool TryGetClient(string clientId, out ConnectionClient client)
        {
            return Clients.TryGetValue(clientId, out client);
        }

        internal void Clear() => Clients.Clear();

        public void Send(string function, params object[] args)
        {
            foreach (string clientId in ClientIds)
            {
                if (Clients.TryGetValue(clientId, out ConnectionClient client))
                    client.Send(function, args);
            }
        }

        public IEnumerator<ConnectionClient> GetEnumerator() => Clients.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Clients.Values.GetEnumerator();
    }
}
