using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;
using PoiApi.Services;

namespace PoiApi.Hubs;

public class UserTrackerHub : Hub
{
    private readonly IUserTrackerService _userTrackerService;
    private readonly AppDbContext _dbContext;

    public UserTrackerHub(IUserTrackerService userTrackerService, AppDbContext dbContext)
    {
        _userTrackerService = userTrackerService;
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        if (IsObserverConnection())
        {
            await Clients.Caller.SendAsync("OnlineUsersUpdated", _userTrackerService.GetOnlineUsers(), _userTrackerService.GetOnlineUserCount());
            await base.OnConnectedAsync();
            return;
        }

        var user = await BuildOnlineUserAsync();
        _userTrackerService.AddOrUpdateConnection(user, Context.ConnectionId);

        await BroadcastOnlineUsersAsync();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!IsObserverConnection())
        {
            _userTrackerService.RemoveConnection(Context.ConnectionId);
            await BroadcastOnlineUsersAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task<IReadOnlyCollection<OnlineUserDto>> GetOnlineUsers()
    {
        return Task.FromResult(_userTrackerService.GetOnlineUsers());
    }

    private async Task<OnlineUserDto> BuildOnlineUserAsync()
    {
        var userId = ResolveUserId();
        var fallbackName = Context.GetHttpContext()?.Request.Query["name"].ToString();
        var fallbackEmail = Context.GetHttpContext()?.Request.Query["email"].ToString();
        var fallbackPackage = NormalizePackageTier(Context.GetHttpContext()?.Request.Query["package"].ToString());

        if (!int.TryParse(userId, out var parsedUserId))
        {
            return new OnlineUserDto
            {
                UserId = string.IsNullOrWhiteSpace(userId) ? $"anon-{Context.ConnectionId}" : userId,
                DisplayName = string.IsNullOrWhiteSpace(fallbackName) ? "Khach tham quan" : fallbackName,
                Email = string.IsNullOrWhiteSpace(fallbackEmail) ? "guest@foodstreet.local" : fallbackEmail,
                ServicePackage = string.IsNullOrWhiteSpace(fallbackPackage) ? "Basic" : fallbackPackage,
                ConnectedAtUtc = DateTime.UtcNow
            };
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == parsedUserId)
            .Select(x => new { x.Id, x.FullName, x.Email })
            .FirstOrDefaultAsync();

        var activeTier = await _dbContext.Subscriptions
            .AsNoTracking()
            .Include(x => x.ServicePackage)
            .Where(x => x.UserId == parsedUserId)
            .Where(x => x.Status == SubscriptionConstants.Active)
            .Where(x => x.EndDate > DateTime.UtcNow)
            .Where(x => x.ServicePackage.Audience == RoleConstants.User)
            .OrderByDescending(x => x.ActivatedAt.HasValue)
            .ThenByDescending(x => x.ActivatedAt)
            .ThenByDescending(x => x.EndDate)
            .Select(x => x.ServicePackage.Tier)
            .FirstOrDefaultAsync();

        return new OnlineUserDto
        {
            UserId = parsedUserId.ToString(),
            DisplayName = !string.IsNullOrWhiteSpace(user?.FullName)
                ? user.FullName
                : (!string.IsNullOrWhiteSpace(fallbackName) ? fallbackName : $"User {parsedUserId}"),
            Email = !string.IsNullOrWhiteSpace(user?.Email)
                ? user.Email
                : (string.IsNullOrWhiteSpace(fallbackEmail) ? $"user{parsedUserId}@foodstreet.local" : fallbackEmail),
            ServicePackage = NormalizePackageTier(activeTier) ?? (string.IsNullOrWhiteSpace(fallbackPackage) ? "Basic" : fallbackPackage),
            ConnectedAtUtc = DateTime.UtcNow
        };
    }

    private async Task BroadcastOnlineUsersAsync()
    {
        var users = _userTrackerService.GetOnlineUsers();
        await Clients.All.SendAsync("OnlineUsersUpdated", users, users.Count);
    }

    private string? ResolveUserId()
    {
        var claimUserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(claimUserId))
        {
            return claimUserId;
        }

        var queryUserId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        if (!string.IsNullOrWhiteSpace(queryUserId))
        {
            return queryUserId;
        }

        return Context.UserIdentifier;
    }

    private static string? NormalizePackageTier(string? tier)
    {
        if (string.IsNullOrWhiteSpace(tier))
        {
            return null;
        }

        var normalized = tier.Trim();
        return normalized.ToLowerInvariant() switch
        {
            "tourbasic" => "Basic",
            "tourplus" => "Premium",
            "basic" => "Basic",
            "premium" => "Premium",
            "vip" => "VIP",
            _ => normalized
        };
    }

    private bool IsObserverConnection()
    {
        var source = Context.GetHttpContext()?.Request.Query["source"].ToString();
        return string.Equals(source, "admin-dashboard", StringComparison.OrdinalIgnoreCase);
    }
}
