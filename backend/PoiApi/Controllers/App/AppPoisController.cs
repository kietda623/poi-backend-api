using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;
using PoiApi.Models;
using PoiApi.Services;
using System.Security.Claims;

[ApiController]
[Route("api/app/pois")]
public class AppPoisController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly AzureTranslationService _translator;
    private readonly SubscriptionAccessService _subscriptionAccessService;

    public AppPoisController(
        AppDbContext context,
        IMapper mapper,
        AzureTranslationService translator,
        SubscriptionAccessService subscriptionAccessService)
    {
        _context = context;
        _mapper = mapper;
        _translator = translator;
        _subscriptionAccessService = subscriptionAccessService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string lang = "vi")
    {
        var canAccessAudio = await CanCurrentUserAccessAudioAsync();
        var shops = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .Where(s => s.IsActive && s.Poi != null)
            .ToListAsync();

        var resultList = new List<AppPoiListDto>();
        var namesToTranslate = new List<string>();
        var shopsNeedTranslation = new List<Tuple<int, string>>();

        foreach (var s in shops)
        {
            var p = s.Poi!;
            var translation = p.Translations.FirstOrDefault(x => x.LanguageCode == lang);

            if (translation != null)
            {
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Location = p.Location ?? string.Empty,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = canAccessAudio
                        ? (translation.AudioUrl ?? p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? string.Empty)
                        : string.Empty,
                    Name = translation.Name
                });
            }
            else if (lang == "vi")
            {
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Location = p.Location ?? string.Empty,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = canAccessAudio
                        ? (p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? string.Empty)
                        : string.Empty,
                    Name = s.Name
                });
            }
            else
            {
                shopsNeedTranslation.Add(new Tuple<int, string>(p.Id, s.Name));
                namesToTranslate.Add(s.Name);

                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    Location = p.Location ?? string.Empty,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = canAccessAudio
                        ? (p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? string.Empty)
                        : string.Empty,
                    Name = s.Name
                });
            }
        }

        if (lang != "vi" && namesToTranslate.Any())
        {
            var translatedNames = await _translator.TranslateListAsync(namesToTranslate, lang);
            if (translatedNames != null && translatedNames.Count == namesToTranslate.Count)
            {
                for (var i = 0; i < shopsNeedTranslation.Count; i++)
                {
                    var id = shopsNeedTranslation[i].Item1;
                    var item = resultList.FirstOrDefault(x => x.Id == id);
                    if (item != null)
                    {
                        item.Name = translatedNames[i];
                    }
                }
            }
        }

        return Ok(resultList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id, [FromQuery] string lang = "vi")
    {
        var canAccessAudio = await CanCurrentUserAccessAudioAsync();
        var shop = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .Include(s => s.Menus)
                .ThenInclude(m => m.MenuItems)
            .FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);

        if (shop == null || shop.Poi == null)
        {
            return NotFound();
        }

        var poi = shop.Poi;
        var requestedTranslation = poi.Translations.FirstOrDefault(x => x.LanguageCode == lang);
        var vietnameseTranslation = poi.Translations.FirstOrDefault(x => x.LanguageCode == "vi");
        var sourceName = vietnameseTranslation?.Name ?? shop.Name;
        var sourceDescription = vietnameseTranslation?.Description ?? shop.Description ?? string.Empty;

        POITranslation translation;
        if (requestedTranslation != null && lang != "vi" && requestedTranslation.Name == sourceName)
        {
            translation = new POITranslation
            {
                LanguageCode = lang,
                Name = await _translator.TranslateAsync(sourceName, lang) ?? sourceName,
                Description = await _translator.TranslateAsync(sourceDescription, lang) ?? sourceDescription,
                AudioUrl = requestedTranslation.AudioUrl
            };
        }
        else if (requestedTranslation != null)
        {
            translation = requestedTranslation;
        }
        else if (lang == "vi")
        {
            translation = new POITranslation
            {
                LanguageCode = "vi",
                Name = sourceName,
                Description = sourceDescription
            };
        }
        else
        {
            translation = new POITranslation
            {
                LanguageCode = lang,
                Name = await _translator.TranslateAsync(sourceName, lang) ?? sourceName,
                Description = await _translator.TranslateAsync(sourceDescription, lang) ?? sourceDescription,
                AudioUrl = string.Empty
            };
        }

        var dto = new AppPoiDetailDto
        {
            Id = poi.Id,
            ImageUrl = poi.ImageUrl ?? string.Empty,
            MenuImagesUrl = poi.MenuImagesUrl,
            Location = poi.Location ?? string.Empty,
            Latitude = poi.Latitude,
            Longitude = poi.Longitude,
            AudioUrl = canAccessAudio
                ? (translation.AudioUrl
                    ?? vietnameseTranslation?.AudioUrl
                    ?? poi.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl
                    ?? string.Empty)
                : string.Empty,
            Name = translation.Name,
            Description = !string.IsNullOrWhiteSpace(translation.Description) ? translation.Description : (shop.Description ?? string.Empty),
            Menus = shop.Menus?
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new AppMenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Items = m.MenuItems
                    .Where(i => i.IsAvailable)
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new AppMenuItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    ImageUrl = i.ImageUrl
                }).ToList()
            }).ToList() ?? new List<AppMenuDto>(),
            AvailableLanguages = new[] { "vi", "en", "zh" }.Select(code =>
            {
                var tr = poi.Translations.FirstOrDefault(x => x.LanguageCode == code);
                return new AppLanguageDto
                {
                    Code = code,
                    Name = GetLanguageName(code),
                    HasAudio = canAccessAudio && tr != null && !string.IsNullOrEmpty(tr.AudioUrl)
                };
            }).ToList()
        };

        if (lang != "vi" && dto.Menus.Any())
        {
            var textsToTranslate = new List<string>();
            foreach (var menu in dto.Menus)
            {
                textsToTranslate.Add(menu.Name);
                foreach (var item in menu.Items)
                {
                    textsToTranslate.Add(item.Name);
                }
            }

            var translatedTexts = await _translator.TranslateListAsync(textsToTranslate, lang);
            if (translatedTexts != null && translatedTexts.Count == textsToTranslate.Count)
            {
                var index = 0;
                foreach (var menu in dto.Menus)
                {
                    menu.Name = translatedTexts[index++];
                    foreach (var item in menu.Items)
                    {
                        item.Name = translatedTexts[index++];
                    }
                }
            }
        }

        return Ok(dto);
    }

    [HttpPost("{id}/view")]
    public async Task<IActionResult> TrackView(int id)
    {
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);
        if (shop == null)
        {
            return NotFound();
        }

        shop.ViewCount++;
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // Cho phép cả USER (đăng nhập) và GUEST (ẩn danh) nghe thuyết minh
    // Kiểm tra quyền audio dựa trên UserId hoặc DeviceId
    [Authorize]
    [HttpPost("{id}/listen")]
    public async Task<IActionResult> TrackListen(int id, [FromQuery] string deviceId = "anonymous")
    {
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);
        if (shop == null)
        {
            return NotFound();
        }

        var isGuest = GuestTokenService.IsGuest(User);
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Kiểm tra quyền audio: User bằng UserId, Guest bằng DeviceId
        if (isGuest)
        {
            var guestDeviceId = GuestTokenService.GetDeviceId(User) ?? deviceId;
            if (!await _subscriptionAccessService.HasAudioAccessByDeviceAsync(guestDeviceId))
            {
                return StatusCode(403, await BuildAudioAccessDeniedForGuestAsync(guestDeviceId));
            }
        }
        else
        {
            if (!int.TryParse(userIdValue, out var userId) || !await _subscriptionAccessService.HasAudioAccessAsync(userId))
            {
                int.TryParse(userIdValue, out var uid);
                return StatusCode(403, await BuildAudioAccessDeniedPayloadAsync(uid));
            }
        }

        shop.ListenCount++;
        shop.ViewCount++;

        // Xác định DeviceId cho usage history
        string usageDeviceId;
        string? guestId = null;
        if (isGuest)
        {
            usageDeviceId = GuestTokenService.GetDeviceId(User) ?? deviceId;
            guestId = GuestTokenService.GetGuestId(User);
        }
        else
        {
            usageDeviceId = !string.IsNullOrWhiteSpace(userIdValue)
                ? $"user:{userIdValue}"
                : deviceId;
        }

        var usage = new UsageHistory
        {
            DeviceId = usageDeviceId,
            GuestId = guestId,
            ShopId = shop.Id,
            ListenedAt = DateTime.UtcNow,
            DurationSeconds = 0
        };
        _context.UsageHistories.Add(usage);

        await _context.SaveChangesAsync();
        return Ok(new { success = true, shop.ListenCount });
    }

    // Cho phép cả User đã đăng nhập và Guest gửi review
    [Authorize]
    [HttpPost("{id}/reviews")]
    public async Task<IActionResult> SubmitReview(int id, [FromBody] AppReviewDto dto)
    {
        var shop = await _context.Shops.FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);
        if (shop == null)
        {
            return NotFound();
        }

        var isGuest = GuestTokenService.IsGuest(User);
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { success = false, message = "Bạn cần xác thực trước khi gửi đánh giá." });
        }

        // Kiểm tra đã nghe chưa: Guest dùng DeviceId, User dùng userId
        bool hasListened;
        if (isGuest)
        {
            var guestDeviceId = GuestTokenService.GetDeviceId(User) ?? "";
            hasListened = await _context.UsageHistories
                .AnyAsync(x => x.ShopId == shop.Id && x.DeviceId == guestDeviceId);
        }
        else
        {
            hasListened = await _context.UsageHistories
                .AnyAsync(x => x.ShopId == shop.Id && x.DeviceId == $"user:{userId}");
        }

        if (!hasListened)
        {
            return StatusCode(403, new { success = false, message = "Bạn cần nghe thuyết minh POI trước khi gửi đánh giá." });
        }

        var review = new Review
        {
            ShopId = shop.Id,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CustomerName = dto.CustomerName ?? (isGuest ? "Khách tham quan" : "Khach hang"),
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Recalculate denormalized AverageRating after new review
        var avgRating = await _context.Reviews
            .Where(r => r.ShopId == shop.Id)
            .AverageAsync(r => (double)r.Rating);
        shop.AverageRating = Math.Round(avgRating, 2);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // Kiểm tra quyền audio: hỗ trợ cả User (UserId) và Guest (DeviceId)
    private async Task<bool> CanCurrentUserAccessAudioAsync()
    {
        // Guest: kiểm tra bằng DeviceId
        if (GuestTokenService.IsGuest(User))
        {
            var deviceId = GuestTokenService.GetDeviceId(User);
            if (string.IsNullOrWhiteSpace(deviceId)) return false;
            return await _subscriptionAccessService.HasAudioAccessByDeviceAsync(deviceId);
        }

        // User đã đăng nhập: kiểm tra bằng UserId
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdValue, out var userId))
        {
            return false;
        }

        return await _subscriptionAccessService.HasAudioAccessAsync(userId);
    }

    private async Task<object> BuildAudioAccessDeniedPayloadAsync(int userId)
    {
        var info = await _subscriptionAccessService.GetUserSubscriptionInfoAsync(userId);
        if (info == null)
        {
            return new
            {
                success = false,
                message = "Bạn chưa có gói Tour đang hoạt động. Hãy thanh toán Tour Basic hoặc Tour Plus và đồng bộ thanh toán để nghe thuyết minh.",
                reason = "no_active_subscription"
            };
        }

        return new
        {
            success = false,
            message = $"Gói hiện tại ({info.PackageName}) chưa cho phép nghe thuyết minh hoặc đã hết hạn. Vui lòng kiểm tra lại trạng thái gói.",
            currentPackage = info.PackageName,
            subscriptionEndDate = info.EndDate,
            reason = "audio_not_available"
        };
    }

    // Payload từ chối audio cho Guest (dựa trên DeviceId)
    private async Task<object> BuildAudioAccessDeniedForGuestAsync(string deviceId)
    {
        var info = await _subscriptionAccessService.GetSubscriptionInfoByDeviceAsync(deviceId);
        if (info == null)
        {
            return new
            {
                success = false,
                message = "Bạn chưa có gói Tour. Hãy mua gói Tour Basic hoặc Tour Plus để nghe thuyết minh.",
                reason = "no_active_subscription"
            };
        }

        return new
        {
            success = false,
            message = $"Gói hiện tại ({info.PackageName}) chưa cho phép nghe thuyết minh hoặc đã hết hạn.",
            currentPackage = info.PackageName,
            subscriptionEndDate = info.EndDate,
            reason = "audio_not_available"
        };
    }

    private static string GetLanguageName(string code) => code.ToLower() switch
    {
        "vi" => "Tieng Viet",
        "en" => "English",
        "zh" => "Chinese",
        _ => code.ToUpperInvariant()
    };
}
