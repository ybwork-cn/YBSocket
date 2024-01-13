using System;

namespace ybwork.YBSocket.Server
{
    public abstract class HubBase : IDisposable
    {
        protected string CurClinetId { get; private set; }
        protected ConnectionClient Client => Clients.GetClient(CurClinetId);
        protected ConnectionClientCollection Clients { get; private set; }
        protected ConnectionGroupCollection Groups { get; private set; }

        public void Dispose()
        {
            foreach (ConnectionClient client in Clients)
            {
                client.Dispose();
            }
            Clients.Clear();
        }
    }
}
