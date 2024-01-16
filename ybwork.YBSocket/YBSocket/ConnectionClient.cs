using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ybwork.YBSocket
{
    public class ConnectionClient : IDisposable
    {
        internal readonly Socket ClientSocket;
        internal readonly byte[] Buffer = new byte[1024 * 1024];
        public readonly string ClinetId;

        private MemoryStream memoryStream = new();

        internal ConnectionClient(Socket clientSocket)
        {
            ClientSocket = clientSocket;
            ClinetId = ClientSocket.Handle.ToInt64().ToString();
        }

        internal bool TryGetMessage(byte[] buffer, int size, out List<string> messages)
        {
            messages = new List<string>();

            // 获取信息长度描述符
            memoryStream.Write(buffer, 0, size);
            byte[] head = new byte[6];
            memoryStream.Position = 0;
            while (memoryStream.Length - memoryStream.Position > 6)
            {
                memoryStream.Read(head, 0, 6);
                string str_head = Encoding.UTF8.GetString(head, 0, 6);
                int dataLength = int.Parse(str_head);
                if (memoryStream.Length - memoryStream.Position < dataLength)
                {
                    memoryStream.Position -= 6;
                    break;
                }

                // 获取真实数据
                byte[] data = new byte[dataLength];
                memoryStream.Read(data, 0, dataLength);
                string json = Encoding.UTF8.GetString(data);
                messages.Add(json);
            }
            if (memoryStream.Position > 0)
            {
                // 删除已处理数据
                byte[] restData = memoryStream.ToArray().Skip((int)memoryStream.Position).ToArray();
                memoryStream = new MemoryStream();
                memoryStream.Write(restData, 0, restData.Length);
            }

            memoryStream.Position = memoryStream.Length;
            return messages.Count > 0;
        }

        internal void Send(string function, params object[] args)
        {
            WebMessage webMessage = new()
            {
                Function = function,
                Params = JArray.FromObject(args)
            };

            byte[] data = GetBuffer(Encoding.UTF8.GetBytes(webMessage.ToString()));
            ClientSocket.Send(data);
        }

        private static byte[] GetBuffer(byte[] data)
        {
            if (data.Length >= 1000000)
                throw new Exception("发送的单条消息不允许大于 1,000,000 字节，请将消息拆分后发送");

            string length = data.Length.ToString().PadLeft(6, '0');
            byte[] headBytes = Encoding.UTF8.GetBytes(length);

            byte[] fullBytes = new byte[data.Length + 6];
            headBytes.CopyTo(fullBytes, 0);
            data.CopyTo(fullBytes, 6);

            return fullBytes;
        }

        public void Dispose()
        {
            ClientSocket?.Close();
            ClientSocket?.Dispose();
            memoryStream.Dispose();
        }
    }
}
