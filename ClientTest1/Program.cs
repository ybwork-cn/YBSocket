using System;
using System.Timers;
using ybwork.YBSocket.Client;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main()
        {
            WebClient webClient = new WebClient("127.0.0.1", 12366);
            webClient.Hub.On<string, int>("Func1", Func1);
            webClient.Connect();
            webClient.Send("UserHub/Join", "aaa");

            Random random = new Random();

            Timer timer = new Timer(100);
            timer.AutoReset = true;
            timer.Elapsed += (_, _) =>
            {
                webClient.Send("MyHub/Func1", "aaa", "user1", random.Next(10));
            };
            timer.Start();

            Console.ReadKey();
        }

        public static void Func1(string name, int type)
        {
            Console.WriteLine(name + "-" + type);
        }
    }
}
