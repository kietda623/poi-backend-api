using PoiApi.Hubs;

namespace PoiApi.Services;

public interface IUserTrackerService
{
    void AddOrUpdateConnection(OnlineUserDto onlineUser, string connectionId);
    void RemoveConnection(string connectionId);
    int GetOnlineUserCount();
    IReadOnlyCollection<string> GetConnections(string userId);
    IReadOnlyCollection<OnlineUserDto> GetOnlineUsers();
}
