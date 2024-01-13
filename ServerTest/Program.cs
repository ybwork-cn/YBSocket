using ybwork.YBSocket.Server;

class Program
{
    static void Main()
    {
        WebServer webServer = new WebServer("127.0.0.1", 12366);
        webServer.BindHub<MyHub>();
        webServer.BindHub<UserHub>();
        webServer.Start();
        Console.ReadKey();
    }
}
