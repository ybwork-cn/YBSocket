﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ybwork.YBSocket.Client
{
    public sealed class WebClient : IDisposable
    {
        readonly ConnectionClient Connection;
        readonly IPEndPoint IPEndPoint;

        public readonly bool IsAutoInvoke;

        public readonly WebActionHub Hub = new();
        private readonly Queue<WebMessage> MessageQueue = new();

        public WebClient(string website, int port, bool isAutoInvoke)
        {
            Regex regex = new(@"^((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))$");
            if (regex.IsMatch(website))
            {
                IPEndPoint = new IPEndPoint(IPAddress.Parse(website), port);
            }
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(website);
                IPEndPoint = new IPEndPoint(hostEntry.AddressList[0], port);
            }

            Socket s = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connection = new ConnectionClient(s);
            IsAutoInvoke = isAutoInvoke;
        }

        public void Connect()
        {
            Socket socket = Connection.ClientSocket;
            byte[] buffer = Connection.Buffer;

            socket.Connect(IPEndPoint);
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, Connection);
        }

        /// <summary>
        /// BeginReceive的回调
        /// </summary>
        /// <param name="result"></param>
        private void Receive(IAsyncResult result)
        {
            Socket socket = Connection.ClientSocket;
            byte[] buffer = Connection.Buffer;

            //string clientIP = socket.RemoteEndPoint.ToString();

            if (!socket.Connected)
                return;
            int dataSize = socket.EndReceive(result);

            if (Connection.TryGetMessage(buffer, dataSize, out List<string> messages))
            {
                foreach (var message in messages)
                {
                    WebMessage webMessage = JsonConvert.DeserializeObject<WebMessage>(message);
                    if (IsAutoInvoke)
                        Hub.Invoke(webMessage);
                    else
                        MessageQueue.Enqueue(webMessage);
                }
            }
            //接收下一条消息
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Receive, Connection);
        }

        public void Send(string function, params object[] args)
        {
            Connection.Send(function, args);
        }

        public void InvokeAllMessages()
        {
            while (MessageQueue.Count > 0)
            {
                WebMessage webMessage = MessageQueue.Dequeue();
                Hub.Invoke(webMessage);
            }
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}
