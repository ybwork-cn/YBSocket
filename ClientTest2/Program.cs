using System;
using ybwork.YBSocket.Client;

namespace Minecraft.ClientTest2
{
    internal class Program
    {
        static void Main()
        {
            WebClient webClient = new WebClient("127.0.0.1", 12366);
            webClient.Hub.On<string, int>("Func1", Func1);
            webClient.Connect();
            webClient.Send("UserHub/Join", "aaa");
            Console.ReadKey();
        }

        public static void Func1(string name, int type)
        {
            Console.WriteLine(name + "-" + type);
        }
    }
}
