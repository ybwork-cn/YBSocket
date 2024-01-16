using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ybwork.YBSocket.Server
{
    public class WebServer : IDisposable
    {
        private readonly Socket Socket;
        private readonly IPEndPoint IPEndPoint;

        private readonly Dictionary<string, HubBase> Hubs = new();
        private readonly ConnectionClientCollection Clients;
        private readonly ConnectionGroupCollection Groups;

        public WebServer(string host, int port)
        {
            Regex regex = new(@"^((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))$");
            if (regex.IsMatch(host))
            {
                IPEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            }
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                IPEndPoint = new IPEndPoint(hostEntry.AddressList[0], port);
            }

            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Clients = new();
            Groups = new ConnectionGroupCollection(Clients);
        }

        public void BindHub<T>() where T : HubBase, new()
        {
            T hub = new T();

            var type = typeof(HubBase);
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            type.GetProperty("Clients", flags).SetValue(hub, Clients);
            type.GetProperty("Groups", flags).SetValue(hub, Groups);

            Hubs.Add(hub.GetType().Name, hub);
        }

        public void Start()
        {
            Socket.Bind(IPEndPoint);
            Socket.Listen(10);
            Socket.BeginAccept(Accept, Socket);
        }

        public void Stop()
        {
            Socket.Close();
        }

        public void Dispose()
        {
            foreach (var hub in Hubs.Values)
                hub.Dispose();
            Hubs.Clear();

            Socket.Dispose();
        }

        /// <summary>
        /// BeginAccept的回调
        /// </summary>
        /// <param name="result"></param>
        private void Accept(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            Socket clientSocket = socket.EndAccept(result);

            // 处理传入事件
            IPEndPoint ipendPoint = (IPEndPoint)clientSocket.RemoteEndPoint;

            //开始接受该客户端连接的消息
            ConnectionClient client = new(clientSocket);
            byte[] buffer = client.Buffer;
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, client);

            //接受下一个连接
            socket.BeginAccept(Accept, socket);
        }

        /// <summary>
        /// BeginReceive的回调
        /// </summary>
        /// <param name="result"></param>
        private void Receive(IAsyncResult result)
        {
            ConnectionClient client = result.AsyncState as ConnectionClient;
            Socket socket = client.ClientSocket;
            byte[] buffer = client.Buffer;
            //string clientIP = socket.RemoteEndPoint.ToString();

            bool disconnected = false;
            try
            {
                int dataSize = socket.EndReceive(result);
                // 返回数据大小为0，视作客户端已关闭连接
                if (dataSize != 0)
                {
                    if (client.TryGetMessage(buffer, dataSize, out List<string> messages))
                    {
                        foreach (string message in messages)
                        {
                            WebMessage webMessage = JsonConvert.DeserializeObject<WebMessage>(message);
                            Invoke(client, webMessage);
                        }
                    }
                    //接收下一条消息
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, client);
                }
                else
                    disconnected = true;
            }
            catch (SocketException)
            {
                disconnected = true;
            }

            if (disconnected)
            {
                Clients.Remove(client);
                Groups.Remove(client.ClinetId);
            }
        }

        private void Invoke(ConnectionClient client, WebMessage webMessage)
        {
            string[] funcPath = webMessage.Function.Split('/');
            if (funcPath.Length != 2)
                return;

            string hubName = funcPath[0];
            string function = funcPath[1];

            if (string.IsNullOrEmpty(hubName))
                return;
            if (!Hubs.TryGetValue(hubName, out HubBase hub))
                return;

            Clients.Add(client);

            MethodInfo method = hub.GetType().GetMethod(function);
            if (method == null)
                return;

            ParameterInfo[] paramTypes = method.GetParameters();
            if (paramTypes.Length != webMessage.Params.Count)
                return;

            object[] paras = new object[paramTypes.Length];
            for (int i = 0; i < webMessage.Params.Count; i++)
            {
                paras[i] = webMessage.Params[i].ToObject(paramTypes[i].ParameterType);
            }
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(HubBase).GetProperty("CurClinetId", flags).SetValue(hub, client.ClinetId);
            method.Invoke(hub, paras);
        }
    }
}
