using ybwork.YBSocket.Server;

class MyHub : HubBase
{
    public void Func1(string roomId, string name, int type)
    {
        Console.WriteLine(roomId + ":" + name + "-" + type);
        // 数据同步至观众玩家
        Groups[roomId].Send(nameof(Func1), name, type);
    }
}

class UserHub : HubBase
{
    public void Join(string roomId)
    {
        Console.WriteLine(CurClinetId);
        Groups.SetGroup(CurClinetId, roomId);
    }
}
