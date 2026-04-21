using System.Collections.Concurrent;
using PoiApi.Hubs;

namespace PoiApi.Services;

public class UserTrackerService : IUserTrackerService
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> UserConnections = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, string> ConnectionUsers = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, OnlineUserDto> OnlineUsers = new(StringComparer.Ordinal);

    public void AddOrUpdateConnection(OnlineUserDto onlineUser, string connectionId)
    {
        if (onlineUser == null || string.IsNullOrWhiteSpace(onlineUser.UserId) || string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }

        var existing = OnlineUsers.GetOrAdd(onlineUser.UserId, _ => onlineUser);

        if (!string.IsNullOrWhiteSpace(onlineUser.DisplayName))
        {
            existing.DisplayName = onlineUser.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(onlineUser.Email))
        {
            existing.Email = onlineUser.Email;
        }

        if (!string.IsNullOrWhiteSpace(onlineUser.ServicePackage))
        {
            existing.ServicePackage = onlineUser.ServicePackage;
        }

        if (existing.ConnectedAtUtc == default)
        {
            existing.ConnectedAtUtc = onlineUser.ConnectedAtUtc == default ? DateTime.UtcNow : onlineUser.ConnectedAtUtc;
        }

        var connections = UserConnections.GetOrAdd(onlineUser.UserId, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
        connections[connectionId] = 0;
        ConnectionUsers[connectionId] = onlineUser.UserId;
    }

    public void RemoveConnection(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }

        if (!ConnectionUsers.TryRemove(connectionId, out var userId))
        {
            return;
        }

        if (!UserConnections.TryGetValue(userId, out var connections))
        {
            return;
        }

        connections.TryRemove(connectionId, out _);
        if (connections.IsEmpty)
        {
            UserConnections.TryRemove(userId, out _);
            OnlineUsers.TryRemove(userId, out _);
        }
    }

    public int GetOnlineUserCount() => UserConnections.Count;

    public IReadOnlyCollection<string> GetConnections(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId) || !UserConnections.TryGetValue(userId, out var connections))
        {
            return Array.Empty<string>();
        }

        return connections.Keys.ToArray();
    }

    public IReadOnlyCollection<OnlineUserDto> GetOnlineUsers()
    {
        return OnlineUsers.Values
            .OrderByDescending(x => x.ConnectedAtUtc)
            .ToList();
    }
}
