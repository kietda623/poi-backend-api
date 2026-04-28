using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;
using PoiApi.Services;

namespace PoiApi.Controllers.App;

[ApiController]
[Route("api/app/tinder")]
[Authorize]
public class TinderController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SubscriptionAccessService _subscriptionAccess;
    private readonly ILogger<TinderController> _logger;

    public TinderController(
        AppDbContext context,
        SubscriptionAccessService subscriptionAccess,
        ILogger<TinderController> logger)
    {
        _context = context;
        _subscriptionAccess = subscriptionAccess;
        _logger = logger;
    }

    [HttpGet("cards")]
    public async Task<IActionResult> GetCards([FromQuery] int count = 10)
    {
        try
        {
            var actor = GetActorContext();
            if (!await HasTinderAccessAsync(actor))
            {
                return StatusCode(403, await BuildTinderAccessDeniedPayloadAsync(actor));
            }

            var swipedShopIds = await BuildSwipeQuery(actor)
                .Select(si => si.ShopId)
                .ToListAsync();

            var cards = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p!.Translations)
                .Include(s => s.Menus)
                    .ThenInclude(m => m.MenuItems)
                .Where(s => s.IsActive && s.Poi != null)
                .Where(s => !swipedShopIds.Contains(s.Id))
                .OrderBy(s => Guid.NewGuid())
                .Take(count)
                .Select(s => new
                {
                    s.Id,
                    Name = s.Poi!.Translations
                        .Where(t => t.LanguageCode == "vi")
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? s.Name,
                    Description = s.Poi!.Translations
                        .Where(t => t.LanguageCode == "vi")
                        .Select(t => t.Description)
                        .FirstOrDefault() ?? s.Description ?? string.Empty,
                    ImageUrl = s.Poi!.ImageUrl ?? string.Empty,
                    Location = s.Address ?? s.Poi.Location ?? string.Empty,
                    s.AverageRating,
                    s.ListenCount,
                    TopItems = s.Menus!
                        .SelectMany(m => m.MenuItems)
                        .Where(mi => mi.IsAvailable)
                        .OrderBy(mi => mi.DisplayOrder)
                        .Take(3)
                        .Select(mi => new { mi.Name, mi.Price, mi.ImageUrl })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                cards,
                remainingCount = await _context.Shops
                    .Where(s => s.IsActive && s.Poi != null && !swipedShopIds.Contains(s.Id))
                    .CountAsync() - cards.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tinder cards");
            return StatusCode(500, new { success = false, message = "Da xay ra loi." });
        }
    }

    [HttpPost("swipe")]
    public async Task<IActionResult> Swipe([FromBody] TinderSwipeRequestDto dto)
    {
        try
        {
            var actor = GetActorContext();
            if (!await HasTinderAccessAsync(actor))
            {
                return StatusCode(403, await BuildTinderAccessDeniedPayloadAsync(actor));
            }

            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == dto.ShopId && s.IsActive);
            if (shop == null)
            {
                return NotFound(new { success = false, message = "Quan an khong tim thay." });
            }

            var existing = await BuildSwipeQuery(actor)
                .FirstOrDefaultAsync(si => si.ShopId == dto.ShopId);

            if (existing != null)
            {
                existing.IsLiked = dto.IsLiked;
                existing.SwipedAt = DateTime.UtcNow;
            }
            else
            {
                _context.SwipedItems.Add(new Models.SwipedItem
                {
                    UserId = actor.UserId,
                    DeviceId = actor.IsGuest ? actor.DeviceId : null,
                    ShopId = dto.ShopId,
                    IsLiked = dto.IsLiked,
                    SwipedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                action = dto.IsLiked ? "liked" : "disliked",
                shopId = dto.ShopId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error swiping");
            return StatusCode(500, new { success = false, message = "Da xay ra loi." });
        }
    }

    [HttpGet("liked")]
    public async Task<IActionResult> GetLikedShops()
    {
        try
        {
            var actor = GetActorContext();
            if (!await HasTinderAccessAsync(actor))
            {
                return StatusCode(403, await BuildTinderAccessDeniedPayloadAsync(actor));
            }

            var likedShops = await BuildSwipeQuery(actor)
                .Include(si => si.Shop)
                    .ThenInclude(s => s.Poi)
                        .ThenInclude(p => p!.Translations)
                .Where(si => si.IsLiked)
                .OrderByDescending(si => si.SwipedAt)
                .Select(si => new
                {
                    si.ShopId,
                    Name = si.Shop.Poi!.Translations
                        .Where(t => t.LanguageCode == "vi")
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? si.Shop.Name,
                    ImageUrl = si.Shop.Poi!.ImageUrl ?? string.Empty,
                    si.Shop.AverageRating,
                    si.SwipedAt
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                count = likedShops.Count,
                shops = likedShops
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching liked shops");
            return StatusCode(500, new { success = false, message = "Da xay ra loi." });
        }
    }

    private IQueryable<Models.SwipedItem> BuildSwipeQuery(RequestActorContext actor)
    {
        var query = _context.SwipedItems.AsQueryable();
        return actor.IsGuest
            ? query.Where(si => si.DeviceId == actor.DeviceId)
            : query.Where(si => si.UserId == actor.UserId);
    }

    private async Task<bool> HasTinderAccessAsync(RequestActorContext actor)
    {
        return actor.IsGuest
            ? await _subscriptionAccess.HasTinderAccessByDeviceAsync(actor.DeviceId!)
            : await _subscriptionAccess.HasTinderAccessAsync(actor.UserId!.Value);
    }

    private RequestActorContext GetActorContext()
    {
        if (GuestTokenService.IsGuest(User))
        {
            var deviceId = GuestTokenService.GetDeviceId(User);
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                throw new InvalidOperationException("Guest session is missing device id.");
            }

            return new RequestActorContext(null, deviceId, true);
        }

        return new RequestActorContext(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), null, false);
    }

    private async Task<object> BuildTinderAccessDeniedPayloadAsync(RequestActorContext actor)
    {
        var info = actor.IsGuest
            ? await _subscriptionAccess.GetSubscriptionInfoByDeviceAsync(actor.DeviceId!)
            : await _subscriptionAccess.GetUserSubscriptionInfoAsync(actor.UserId!.Value);

        if (info == null)
        {
            return new
            {
                success = false,
                message = "Ban chua co goi Tour dang hoat dong. Hay thanh toan Tour Plus roi thu lai.",
                requiredPackage = "Tour Plus",
                reason = "no_active_subscription"
            };
        }

        return new
        {
            success = false,
            message = $"Goi hien tai ({info.PackageName}) khong bao gom Tinder Am Thuc hoac da het han. Ban can Tour Plus con han de su dung tinh nang nay.",
            requiredPackage = "Tour Plus",
            currentPackage = info.PackageName,
            subscriptionEndDate = info.EndDate,
            reason = "feature_not_in_package"
        };
    }

    private sealed record RequestActorContext(int? UserId, string? DeviceId, bool IsGuest);
}
