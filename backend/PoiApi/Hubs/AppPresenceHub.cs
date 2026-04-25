using System.Threading;
using Microsoft.AspNetCore.SignalR;

namespace PoiApi.Hubs;

public class AppPresenceHub : Hub
{
    private static int _onlineCount;

    public override async Task OnConnectedAsync()
    {
        var count = Interlocked.Increment(ref _onlineCount);
        await Clients.All.SendAsync("OnlineCountUpdated", count);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var decremented = Interlocked.Decrement(ref _onlineCount);
        if (decremented < 0)
        {
            Interlocked.Exchange(ref _onlineCount, 0);
        }

        await Clients.All.SendAsync("OnlineCountUpdated", GetOnlineCount());
        await base.OnDisconnectedAsync(exception);
    }

    public static int GetOnlineCount()
    {
        return Math.Max(0, Volatile.Read(ref _onlineCount));
    }
}
