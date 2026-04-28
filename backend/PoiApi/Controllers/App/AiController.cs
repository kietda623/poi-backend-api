using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;
using PoiApi.Models;
using PoiApi.Services;

namespace PoiApi.Controllers.App;

/// <summary>
/// AI-powered endpoints: Tour Plan generation and "Tho Dia" Chatbot.
/// Both require Tour Plus subscription.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly GroqService _ai;
    private readonly SubscriptionAccessService _subscriptionAccess;
    private readonly ILogger<AiController> _logger;

    public AiController(
        AppDbContext context,
        GroqService ai,
        SubscriptionAccessService subscriptionAccess,
        ILogger<AiController> logger)
    {
        _context = context;
        _ai = ai;
        _subscriptionAccess = subscriptionAccess;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/ai/tour-plan
    /// Nhận danh sách shop đã quẹt phải (Tinder), kết hợp gợi ý thêm quán từ DB,
    /// gọi Groq tạo lịch trình ăn uống.
    /// Logic lọc: Chỉ gợi ý thêm quán có Seller VIP/Premium VÀ Rating >= 4.7
    /// </summary>
    [HttpPost("tour-plan")]
    public async Task<IActionResult> GenerateTourPlan([FromBody] AiTourPlanRequestDto dto)
    {
        try
        {
            var actor = GetActorContext();
            var userId = actor.UserId ?? 0;

            if (!(actor.IsGuest
                ? await _subscriptionAccess.HasAiPlanAccessByDeviceAsync(actor.DeviceId!)
                : await _subscriptionAccess.HasAiPlanAccessAsync(actor.UserId!.Value)))
            {
                return StatusCode(403, await BuildAiAccessDeniedPayloadAsync(actor, "ai_plan"));
            }

            if (dto.LikedShopIds == null || !dto.LikedShopIds.Any())
            {
                return BadRequest(new { success = false, message = "Vui lòng quẹt thích ít nhất 1 quán ăn trước khi tạo lịch trình." });
            }

            // Bước 1: Lấy thông tin các quán mà user đã quẹt phải (liked)
            var likedShops = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p!.Translations)
                .Include(s => s.Menus)
                    .ThenInclude(m => m.MenuItems)
                .Where(s => dto.LikedShopIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            // Bước 2: Gợi ý thêm quán - CHỈ lấy quán có Seller VIP/Premium + Rating >= 4.7
            // Đây là logic lọc quan trọng nhất theo yêu cầu nghiệp vụ
            var now = DateTime.UtcNow;
            var suggestedShops = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p!.Translations)
                .Include(s => s.Menus)
                    .ThenInclude(m => m.MenuItems)
                .Where(s => s.IsActive
                    && s.AverageRating >= 4.7  // Điều kiện 1: Rating >= 4.7
                    && !dto.LikedShopIds.Contains(s.Id)) // Loại trừ quán đã quẹt
                .Where(s => _context.Subscriptions
                    .Any(sub =>
                        sub.UserId == s.OwnerId
                        && sub.Status == SubscriptionConstants.Active
                        && sub.EndDate > now
                        && sub.ServicePackage.Audience == RoleConstants.Owner
                        // Điều kiện 2: Seller phải là VIP hoặc Premium
                        && (sub.ServicePackage.Tier == "VIP" || sub.ServicePackage.Tier == "Premium")))
                .OrderByDescending(s => s.AverageRating)
                .ThenByDescending(s => s.ListenCount)
                .Take(10)
                .ToListAsync();

            // Bước 3: Xây dựng context cho AI
            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("=== QUÁN ĂN USER ĐÃ THÍCH ===");
            foreach (var shop in likedShops)
            {
                AppendShopContext(contextBuilder, shop, "LIKED");
            }

            if (suggestedShops.Any())
            {
                contextBuilder.AppendLine("\n=== QUÁN ĂN GỢI Ý THÊM (Rating cao + Seller VIP/Premium) ===");
                foreach (var shop in suggestedShops)
                {
                    AppendShopContext(contextBuilder, shop, "SUGGESTED");
                }
            }

            // Bước 4: System Prompt for Groq AI (language-aware)
            var lang = (dto.Language ?? "vi").ToLowerInvariant();
            var systemPrompt = GetTourPlanSystemPrompt(lang);

            var userMessage = $"Hãy tạo lịch trình ăn uống cho tôi.\n{(string.IsNullOrEmpty(dto.Preferences) ? "" : $"Sở thích thêm: {dto.Preferences}\n")}\nDữ liệu quán ăn:\n{contextBuilder}";

            // Bước 5: Gọi Groq AI
            var aiResponse = await _ai.GenerateContentAsync(systemPrompt, userMessage);

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                _logger.LogWarning("Groq unavailable for tour-plan. Returning deterministic fallback plan for user {UserId}.", userId);
                aiResponse = BuildFallbackTourPlan(likedShops, suggestedShops, dto.Preferences);
            }
            else
            {
                aiResponse = SanitizeTourPlanReply(aiResponse);
                if (string.IsNullOrWhiteSpace(aiResponse) || LooksLikePromptLeak(aiResponse))
                {
                    _logger.LogWarning("Detected prompt leak-like tour-plan response. Switching to fallback plan for user {UserId}.", userId);
                    aiResponse = BuildFallbackTourPlan(likedShops, suggestedShops, dto.Preferences);
                }
            }

            return Ok(new
            {
                success = true,
                tourPlan = aiResponse,
                likedShopCount = likedShops.Count,
                suggestedShopCount = suggestedShops.Count,
                totalShopsInPlan = likedShops.Count + suggestedShops.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tour plan");
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi tạo lịch trình." });
        }
    }

    /// <summary>
    /// POST /api/ai/chatbot
    /// Chatbot "Thổ Địa" - Tư vấn món ăn dựa trên DB context (Top Rating + Top ListenCount).
    /// </summary>
    [HttpPost("chatbot")]
    public async Task<IActionResult> Chat([FromBody] AiChatbotRequestDto dto)
    {
        try
        {
            var actor = GetActorContext();
            var userId = actor.UserId ?? 0;

            if (!(actor.IsGuest
                ? await _subscriptionAccess.HasChatbotAccessByDeviceAsync(actor.DeviceId!)
                : await _subscriptionAccess.HasChatbotAccessAsync(actor.UserId!.Value)))
            {
                return StatusCode(403, await BuildAiAccessDeniedPayloadAsync(actor, "chatbot"));
            }

            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { success = false, message = "Tin nhắn không được để trống." });
            }

            // 1. Tìm kiếm quán cụ thể dựa trên từ khóa trong tin nhắn
            var searchResults = new List<Shop>();
            var userMsgLower = dto.Message.ToLower();
            
            // Lấy danh sách tất cả các quán đang hoạt động (cache đơn giản hoặc query nhanh)
            var allActiveShops = await _context.Shops
                .Include(s => s.Poi).ThenInclude(p => p!.Translations)
                .Include(s => s.Menus).ThenInclude(m => m.MenuItems)
                .Where(s => s.IsActive)
                .ToListAsync();

            // Lọc các quán có tên xuất hiện trong tin nhắn người dùng
            searchResults = allActiveShops
                .Where(s => 
                {
                    var viName = s.Poi?.Translations.FirstOrDefault(t => t.LanguageCode == "vi")?.Name?.ToLower() ?? s.Name?.ToLower();
                    return !string.IsNullOrEmpty(viName) && userMsgLower.Contains(viName);
                })
                .ToList();

            // 2. Lấy thêm Top 10 quán nổi bật làm background context nếu chưa đủ
            var topShops = allActiveShops
                .Where(s => !searchResults.Any(r => r.Id == s.Id))
                .OrderByDescending(s => s.AverageRating)
                .ThenByDescending(s => s.ListenCount)
                .Take(10)
                .ToList();

            // Hợp nhất danh sách
            var allContextShops = searchResults.Concat(topShops).ToList();

            // Xây dựng context từ DB cho System Prompt
            var dbContext = new StringBuilder();
            dbContext.AppendLine("=== DANH SÁCH QUÁN ĂN LIÊN QUAN (DỮ LIỆU CHÍNH XÁC) ===");
            foreach (var shop in allContextShops)
            {
                AppendShopContext(dbContext, shop, searchResults.Contains(shop) ? "KẾT QUẢ TÌM KIẾM" : "TOP ĐỀ XUẤT");
            }

            // System prompt - language-aware for multilingual chatbot
            var lang = (dto.Language ?? "vi").ToLowerInvariant();
            var systemPrompt = GetChatbotSystemPrompt(lang, dbContext.ToString());

            // Convert history DTO to GroqChatTurn
            var history = dto.History?.Select(h => new GroqChatTurn
            {
                Role = h.Role == "model" ? "assistant" : "user",
                Message = h.Message
            }).ToList();

            var aiResponse = await _ai.GenerateContentAsync(systemPrompt, dto.Message, history);

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                _logger.LogWarning("Groq unavailable for chatbot. Returning deterministic fallback answer for user {UserId}.", userId);
                aiResponse = BuildFallbackChatReply(dto.Message, topShops);
            }
            else
            {
                aiResponse = SanitizeChatReply(aiResponse);
                if (string.IsNullOrWhiteSpace(aiResponse) || LooksLikePromptLeak(aiResponse))
                {
                    _logger.LogWarning("Detected prompt leak-like chatbot response. Switching to fallback reply for user {UserId}.", userId);
                    aiResponse = BuildFallbackChatReply(dto.Message, topShops);
                }
            }

            return Ok(new
            {
                success = true,
                reply = aiResponse
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chatbot");
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi." });
        }
    }

    /// <summary>
    /// GET /api/ai/subscription-info
    /// Returns current user subscription tier and feature access for MAUI client gate.
    /// </summary>
    [HttpGet("subscription-info")]
    public async Task<IActionResult> GetSubscriptionInfo()
    {
        try
        {
            var actor = GetActorContext();
            var userId = actor.UserId ?? 0;
            _logger.LogInformation("Getting AI subscription info for actor. IsGuest={IsGuest}, UserId={UserId}, DeviceId={DeviceId}", actor.IsGuest, actor.UserId, actor.DeviceId);
            
            var info = actor.IsGuest
                ? await _subscriptionAccess.GetSubscriptionInfoByDeviceAsync(actor.DeviceId!)
                : await _subscriptionAccess.GetUserSubscriptionInfoAsync(actor.UserId!.Value);

            if (info == null)
            {
                _logger.LogWarning("No active subscription found for actor. IsGuest={IsGuest}, UserId={UserId}, DeviceId={DeviceId}", actor.IsGuest, actor.UserId, actor.DeviceId);
                return Ok(new AiSubscriptionResponseDto
                {
                    HasSubscription = false,
                    Tier = "Free",
                    AllowAudio = false,
                    AllowTinder = false,
                    AllowAiPlan = false,
                    AllowChatbot = false
                });
            }

            _logger.LogInformation("Actor has active package {PackageName} (Tier: {Tier}). Flags: Tinder={AllowTinder}, AiPlan={AllowAiPlan}, Chatbot={AllowChatbot}", 
                info.PackageName, info.Tier, info.AllowTinder, info.AllowAiPlan, info.AllowChatbot);

            return Ok(new AiSubscriptionResponseDto
            {
                HasSubscription = true,
                Tier = info.Tier,
                PackageName = info.PackageName,
                EndDate = info.EndDate,
                AllowAudio = info.AllowAudio,
                AllowTinder = info.AllowTinder,
                AllowAiPlan = info.AllowAiPlan,
                AllowChatbot = info.AllowChatbot
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI subscription info");
            return StatusCode(500, "Internal server error");
        }
    }

    // ===== HELPER METHODS =====

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

    private async Task<object> BuildAiAccessDeniedPayloadAsync(RequestActorContext actor, string feature)
    {
        var info = actor.IsGuest
            ? await _subscriptionAccess.GetSubscriptionInfoByDeviceAsync(actor.DeviceId!)
            : await _subscriptionAccess.GetUserSubscriptionInfoAsync(actor.UserId!.Value);
        if (info == null)
        {
            return new
            {
                success = false,
                message = "Bạn chưa có gói Tour đang hoạt động. Hãy hoàn tất thanh toán Tour Plus rồi thử lại.",
                requiredPackage = "Tour Plus",
                feature,
                reason = "no_active_subscription"
            };
        }

        return new
        {
            success = false,
            message = $"Gói hiện tại ({info.PackageName}) chưa hỗ trợ tính năng này hoặc đã hết hạn. Cần gói Tour Plus còn hạn.",
            requiredPackage = "Tour Plus",
            currentPackage = info.PackageName,
            subscriptionEndDate = info.EndDate,
            feature,
            reason = "feature_not_in_package"
        };
    }

    private sealed record RequestActorContext(int? UserId, string? DeviceId, bool IsGuest);

    /// <summary>Append shop info to context StringBuilder for AI prompt injection</summary>
    private static void AppendShopContext(StringBuilder sb, Shop shop, string tag)
    {
        var viTranslation = shop.Poi?.Translations.FirstOrDefault(t => t.LanguageCode == "vi");
        var name = viTranslation?.Name ?? shop.Name;
        var description = viTranslation?.Description ?? shop.Description ?? "";

        sb.AppendLine($"\n[{tag}] {name}");
        sb.AppendLine($"  Địa chỉ: {shop.Address ?? shop.Poi?.Location ?? "N/A"}");
        sb.AppendLine($"  Mô tả: {description}");
        sb.AppendLine($"  Rating: {shop.AverageRating:F1}/5 | Lượt nghe: {shop.ListenCount}");

        if (shop.Poi?.Latitude != null && shop.Poi?.Longitude != null)
        {
            sb.AppendLine($"  Tọa độ: {shop.Poi.Latitude}, {shop.Poi.Longitude}");
        }

        // Append menu items
        if (shop.Menus != null)
        {
            foreach (var menu in shop.Menus)
            {
                foreach (var item in menu.MenuItems)
                {
                    sb.AppendLine($"  - Món: {item.Name} | Giá: {item.Price:N0} VNĐ{(item.IsAvailable ? "" : " (Hết)")}");
                }
            }
        }
    }

    private static string BuildFallbackTourPlan(List<Shop> likedShops, List<Shop> suggestedShops, string? preferences)
    {
        var selected = likedShops
            .Concat(suggestedShops)
            .DistinctBy(s => s.Id)
            .Take(4)
            .ToList();

        if (!selected.Any())
        {
            return "Hiện chưa đủ dữ liệu quán để lập lịch trình. Bạn hãy quẹt thích thêm vài quán trong Tinder rồi thử lại.";
        }

        var slots = new[] { "10:00", "12:00", "15:30", "18:30" };
        var sb = new StringBuilder();
        sb.AppendLine("Lịch trình gợi ý (chế độ dự phòng khi AI bận):");
        if (!string.IsNullOrWhiteSpace(preferences))
        {
            sb.AppendLine($"Sở thích bạn đã chọn: {preferences}");
        }

        decimal totalMin = 0;
        decimal totalMax = 0;
        for (var i = 0; i < selected.Count; i++)
        {
            var shop = selected[i];
            var vi = shop.Poi?.Translations?.FirstOrDefault(t => t.LanguageCode == "vi");
            var name = vi?.Name ?? shop.Name;
            var address = shop.Address ?? shop.Poi?.Location ?? "N/A";
            var items = shop.Menus?
                .SelectMany(m => m.MenuItems)
                .Where(m => m.IsAvailable)
                .OrderBy(m => m.DisplayOrder)
                .Take(2)
                .ToList() ?? new List<MenuItem>();

            var minPrice = items.Any() ? items.Min(x => x.Price) : 30000;
            var maxPrice = items.Any() ? items.Max(x => x.Price) : 80000;
            totalMin += minPrice;
            totalMax += maxPrice;

            var dish = items.Any()
                ? string.Join(", ", items.Select(x => x.Name))
                : "Món đặc trưng của quán";

            sb.AppendLine($"{slots[Math.Min(i, slots.Length - 1)]} - {name}");
            sb.AppendLine($"  Địa chỉ: {address}");
            sb.AppendLine($"  Nên thử: {dish}");
            sb.AppendLine($"  Ước tính: {minPrice:N0} - {maxPrice:N0} VNĐ");
        }

        sb.AppendLine($"Tổng chi phí dự kiến: {totalMin:N0} - {totalMax:N0} VNĐ");
        sb.AppendLine("Mẹo: đi theo thứ tự trên để tiết kiệm thời gian di chuyển.");
        return sb.ToString();
    }

    private static string BuildFallbackChatReply(string userMessage, List<Shop> topShops)
    {
        var normalized = (userMessage ?? string.Empty).ToLowerInvariant();
        var compact = normalized.Trim();

        if (IsGreetingMessage(compact))
        {
            return "Chao ban, minh la Tho Dia. Ban muon minh goi y mon gi, quan nao hay khu vuc nao?";
        }

        var keywordMatches = topShops
            .Where(s =>
            {
                var vi = s.Poi?.Translations?.FirstOrDefault(t => t.LanguageCode == "vi");
                var text = $"{vi?.Name} {vi?.Description} {s.Name} {s.Description}".ToLowerInvariant();
                if (normalized.Contains("oc") || normalized.Contains("ốc")) return text.Contains("oc") || text.Contains("ốc");
                if (normalized.Contains("lau") || normalized.Contains("lẩu")) return text.Contains("lau") || text.Contains("lẩu");
                if (normalized.Contains("nuong") || normalized.Contains("nướng")) return text.Contains("nuong") || text.Contains("nướng");
                if (normalized.Contains("com") || normalized.Contains("cơm")) return text.Contains("com") || text.Contains("cơm");
                if (normalized.Contains("che") || normalized.Contains("chè")) return text.Contains("che") || text.Contains("chè");
                return false;
            })
            .Take(3)
            .ToList();

        var picks = keywordMatches.Any()
            ? keywordMatches
            : topShops.Take(3).ToList();

        if (!picks.Any())
        {
            return "Minh chua lay duoc du lieu quan luc nay. Ban thu lai sau it phut nhe.";
        }

        var sb = new StringBuilder();
        if (normalized.Contains("dia chi") || normalized.Contains("địa chỉ"))
        {
            sb.AppendLine("Minh tra dung dia chi theo quan ban quan tam ne:");
        }
        else if (normalized.Contains("lau") || normalized.Contains("lẩu") || normalized.Contains("cay"))
        {
            sb.AppendLine("Ban thich mon dam vi ha, minh goi y vai quan noi bat ne:");
        }
        else if (normalized.Contains("re") || normalized.Contains("rẻ") || normalized.Contains("tiet kiem") || normalized.Contains("tiết kiệm"))
        {
            sb.AppendLine("Neu muon an ngon ma hop tui tien, ban tham khao cac quan sau:");
        }
        else
        {
            sb.AppendLine("Minh goi y nhanh vai quan phu hop cho ban:");
        }

        foreach (var shop in picks)
        {
            var vi = shop.Poi?.Translations?.FirstOrDefault(t => t.LanguageCode == "vi");
            var name = vi?.Name ?? shop.Name;
            var address = shop.Address ?? shop.Poi?.Location ?? "N/A";
            sb.AppendLine($"- {name} ({shop.AverageRating:F1} sao, {shop.ListenCount} luot nghe) - {address}");
        }

        sb.Append("Ban muon minh goi y theo mon cu the nhu oc, nuong, lau hay trang mieng khong?");
        return sb.ToString();
    }

    private static bool IsGreetingMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        var greetings = new[]
        {
            "chao",
            "chào",
            "hello",
            "hi",
            "hey",
            "xin chao",
            "xin chào"
        };

        return greetings.Any(greeting => string.Equals(message, greeting, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikePromptLeak(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.ToLowerInvariant();
        return normalized.Contains("quy tắc trả lời") ||
               normalized.Contains("dữ liệu quán ăn") ||
               normalized.Contains("user says") ||
               normalized.Contains("persona:") ||
               normalized.Contains("goal:") ||
               normalized.Contains("steer conversation") ||
               normalized.Contains("word count") ||
               normalized.Contains("respond to the greeting") ||
               normalized.Contains("you are") ||
               normalized.Contains("system prompt") ||
               normalized.Contains("prioritize suggested list");
    }

    private static string SanitizeChatReply(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var bannedLineHints = new[]
        {
            "user says",
            "persona:",
            "goal:",
            "respond to",
            "steer conversation",
            "word count",
            "quy tắc",
            "dữ liệu quán ăn",
            "system prompt",
            "analysis"
        };

        var cleanLines = text
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !bannedLineHints.Any(h => line.Contains(h, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return string.Join("\n", cleanLines).Trim();
    }

    private static string SanitizeTourPlanReply(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var bannedLineHints = new[]
        {
            "self-correction",
            "drafting",
            "wait, looking at",
            "finalizing details",
            "structure:",
            "priority liked",
            "geographic order",
            "format correct",
            "tone?",
            "note:",
            "prompt asks",
            "analysis",
            "quy tắc",
            "system prompt"
        };

        var cleanLines = text
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !bannedLineHints.Any(h => line.Contains(h, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return string.Join("\n", cleanLines).Trim();
    }

    private static string GetChatbotSystemPrompt(string lang, string dbContext)
    {
        return lang switch
        {
            "en" => $@"You are ""Tho Dia Vinh Khanh"", a street food advisor chatbot in Vinh Khanh Street, District 4, Ho Chi Minh City, Vietnam.

GOAL:
- Answer DIRECTLY to the user's question. If asked about a specific restaurant, provide its info.
- Provide: Restaurant name, Address, Signature menu items and PRICES from the data below.
- NEVER say ""I don't have menu/price info"" if the restaurant has data in the list below.
- If a restaurant is listed but has no detailed menu, say: ""This restaurant hasn't updated its detailed menu on the system yet, but it's famous for [Description]. You should check it out!""

STYLE:
- Friendly, knowledgeable, like a local food guide.
- Keep answers concise (under 150 words).
- ALWAYS respond in English.

SYSTEM DATA:
{dbContext}",

            "zh" => $@"你是""土地公永庆""，越南胡志明市第四区永庆街的美食顾问聊天机器人。

目标：
- 直接回答用户的问题。如果问到具体餐厅，请提供该餐厅的信息。
- 提供：餐厅名称、地址、招牌菜品和价格（来自以下数据）。
- 如果餐厅在列表中有数据，绝对不要说""我没有菜单/价格信息""。
- 如果餐厅在列表中但没有详细菜单，请说：""该餐厅尚未在系统上更新详细菜单，但以[描述]闻名，您应该去试试！""

风格：
- 友好、专业，像当地美食向导。
- 回答简洁（150字以内）。
- 始终用中文回答。

系统数据：
{dbContext}",

            _ => $@"Bạn là ""Thổ Địa Vĩnh Khánh"", chatbot tư vấn ăn uống sành sỏi tại đường Vĩnh Khánh, Quận 4, TP.HCM.

MỤC TIÊU:
- Trả lời ĐÚNG TRỌNG TÂM câu hỏi. Nếu khách hỏi về quán cụ thể, hãy cung cấp thông tin quán đó.
- Cung cấp: Tên quán, Địa chỉ, Menu tiêu biểu và GIÁ CẢ có trong dữ liệu bên dưới.
- TUYỆT ĐỐI KHÔNG nói ""tôi không có thông tin menu/giá"" nếu quán đó có thông tin trong danh sách dưới đây. 
- Nếu quán có trong danh sách nhưng không có menu chi tiết, hãy nói: ""Quán hiện chưa cập nhật menu chi tiết trên hệ thống nhưng rất nổi tiếng với các món [Mô tả quán], bạn nên ghé thử nhé"".

PHONG CÁCH:
- Thân thiện, am hiểu, dùng từ ngữ local (Ví dụ: ""Sài Gòn"", ""Quận 4"", ""ngon nhức nách"", ""giá rẻ bất ngờ"").
- Trả lời ngắn gọn (dưới 150 từ).

DỮ LIỆU HỆ THỐNG CUNG CẤP:
{dbContext}"
        };
    }

    private static string GetTourPlanSystemPrompt(string lang)
    {
        return lang switch
        {
            "en" => @"You are a street food expert in the Vinh Khanh area, Ho Chi Minh City, Vietnam.
Task: Create a detailed food itinerary for tourists based on the restaurant list below.

RULES:
1. Prioritize restaurants the user LIKED as main stops.
2. Include SUGGESTED restaurants if appropriate.
3. Arrange the itinerary logically by geographic proximity.
4. Suggest appropriate times (morning/lunch/afternoon/evening) for each dish type.
5. For each stop: restaurant name, address, must-try dishes, price range.
6. Respond in English, friendly style like a local guide.
7. Estimate total cost at the end.",

            "zh" => @"你是越南胡志明市永庆区的街头美食专家。
任务：根据以下餐厅列表，为游客创建详细的美食行程。

规则：
1. 优先安排用户喜欢（LIKED）的餐厅作为主要站点。
2. 适当加入推荐（SUGGESTED）的餐厅。
3. 按地理位置合理安排行程。
4. 根据菜品类型建议合适的时间（早/午/下午/晚）。
5. 每个站点记录：餐厅名称、地址、必尝菜品、价格范围。
6. 用中文回答，风格友好如当地导游。
7. 最后估算总费用。",

            _ => @"Bạn là chuyên gia ẩm thực đường phố tại khu vực Vĩnh Khánh, TP.HCM.
Nhiệm vụ: Tạo lịch trình ăn uống chi tiết cho khách du lịch dựa trên danh sách quán ăn bên dưới.

QUY TẮC:
1. Ưu tiên các quán mà user đã THÍCH (LIKED) làm điểm chính.
2. Kết hợp thêm các quán GỢI Ý (SUGGESTED) nếu phù hợp.
3. Sắp xếp lịch trình hợp lý theo vị trí địa lý (gần nhau đi trước).
4. Gợi ý thời gian (sáng/trưa/chiều/tối) phù hợp với loại món.
5. Mỗi điểm dừng ghi: tên quán, địa chỉ, món nên thử, mức giá.
6. Trả lời bằng tiếng Việt, phong cách thân thiện như hướng dẫn viên local.
7. Cuối cùng estimate tổng chi phí."
        };
    }
}


