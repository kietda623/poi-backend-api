using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;
using PoiApi.Models;
using PoiApi.Services;

namespace PoiApi.Controllers.App;

/// <summary>
/// Tinder-style food swiping feature.
/// Requires Tour Plus subscription.
/// </summary>
[ApiController]
[Route("api/app/tinder")]
[Authorize(Roles = RoleConstants.User)]
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

    /// <summary>
    /// GET /api/app/tinder/cards
    /// Lấy danh sách shop chưa quẹt (random order), trả về dạng card.
    /// </summary>
    [HttpGet("cards")]
    public async Task<IActionResult> GetCards([FromQuery] int count = 10)
    {
        try
        {
            var userId = GetUserId();

            if (!await _subscriptionAccess.HasTinderAccessAsync(userId))
            {
                return StatusCode(403, await BuildTinderAccessDeniedPayloadAsync(userId));
            }

            // Lấy danh sách ShopId đã quẹt rồi
            var swipedShopIds = await _context.SwipedItems
                .Where(si => si.UserId == userId)
                .Select(si => si.ShopId)
                .ToListAsync();

            // Lấy các shop chưa quẹt, có POI (có hình ảnh / vị trí)
            var cards = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p!.Translations)
                .Include(s => s.Menus)
                    .ThenInclude(m => m.MenuItems)
                .Where(s => s.IsActive && s.Poi != null)
                .Where(s => !swipedShopIds.Contains(s.Id))
                .OrderBy(s => Guid.NewGuid()) // Random order
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
                        .FirstOrDefault() ?? s.Description ?? "",
                    ImageUrl = s.Poi!.ImageUrl ?? "",
                    Location = s.Address ?? s.Poi.Location ?? "",
                    s.AverageRating,
                    s.ListenCount,
                    // Top 3 menu items as preview
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
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi." });
        }
    }

    /// <summary>
    /// POST /api/app/tinder/swipe
    /// Lưu kết quả quẹt trái (dislike) / phải (like).
    /// </summary>
    [HttpPost("swipe")]
    public async Task<IActionResult> Swipe([FromBody] TinderSwipeRequestDto dto)
    {
        try
        {
            var userId = GetUserId();

            if (!await _subscriptionAccess.HasTinderAccessAsync(userId))
            {
                return StatusCode(403, await BuildTinderAccessDeniedPayloadAsync(userId));
            }

            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == dto.ShopId && s.IsActive);
            if (shop == null)
            {
                return NotFound(new { success = false, message = "Quán ăn không tìm thấy." });
            }

            // Upsert: nếu đã quẹt rồi thì cập nhật, chưa thì tạo mới
            var existing = await _context.SwipedItems
                .FirstOrDefaultAsync(si => si.UserId == userId && si.ShopId == dto.ShopId);

            if (existing != null)
            {
                existing.IsLiked = dto.IsLiked;
                existing.SwipedAt = DateTime.UtcNow;
            }
            else
            {
                _context.SwipedItems.Add(new SwipedItem
                {
                    UserId = userId,
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
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi." });
        }
    }

    /// <summary>
    /// GET /api/app/tinder/liked
    /// Lấy danh sách quán đã thích (để đưa vào AI Tour Plan).
    /// </summary>
    [HttpGet("liked")]
    public async Task<IActionResult> GetLikedShops()
    {
        try
        {
            var userId = GetUserId();

            if (!await _subscriptionAccess.HasTinderAccessAsync(userId))
            {
                return StatusCode(403, await BuildTinderAccessDeniedPayloadAsync(userId));
            }

            var likedShops = await _context.SwipedItems
                .Include(si => si.Shop)
                    .ThenInclude(s => s.Poi)
                        .ThenInclude(p => p!.Translations)
                .Where(si => si.UserId == userId && si.IsLiked)
                .OrderByDescending(si => si.SwipedAt)
                .Select(si => new
                {
                    si.ShopId,
                    Name = si.Shop.Poi!.Translations
                        .Where(t => t.LanguageCode == "vi")
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? si.Shop.Name,
                    ImageUrl = si.Shop.Poi!.ImageUrl ?? "",
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
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi." });
        }
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<object> BuildTinderAccessDeniedPayloadAsync(int userId)
    {
        var info = await _subscriptionAccess.GetUserSubscriptionInfoAsync(userId);
        if (info == null)
        {
            return new
            {
                success = false,
                message = "Bạn chưa có gói Tour đang hoạt động. Hãy thanh toán Tour Plus và đồng bộ thanh toán để sử dụng Tinder Ẩm Thực.",
                requiredPackage = "Tour Plus",
                reason = "no_active_subscription"
            };
        }

        return new
        {
            success = false,
            message = $"Gói hiện tại ({info.PackageName}) không bao gồm Tinder Ẩm Thực. Bạn cần Tour Plus còn hạn để sử dụng tính năng này.",
            requiredPackage = "Tour Plus",
            currentPackage = info.PackageName,
            subscriptionEndDate = info.EndDate,
            reason = "feature_not_in_package"
        };
    }
}
