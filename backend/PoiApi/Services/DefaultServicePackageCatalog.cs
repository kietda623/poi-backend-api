using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Services;

public static class DefaultServicePackageCatalog
{
    public static async Task SyncAsync(AppDbContext context)
    {
        await NormalizeLegacyTourPackageTiersAsync(context);

        var defaults = GetDefaults().ToList();

        foreach (var item in defaults)
        {
            var existing = await context.ServicePackages
                .FirstOrDefaultAsync(x => x.Audience == item.Audience && x.Tier == item.Tier);

            if (existing == null)
            {
                context.ServicePackages.Add(item);
                continue;
            }

            existing.Name = item.Name;
            existing.MonthlyPrice = item.MonthlyPrice;
            existing.YearlyPrice = item.YearlyPrice;
            existing.Description = item.Description;
            existing.Features = item.Features;
            existing.MaxStores = item.MaxStores;
            existing.AllowAudioAccess = item.AllowAudioAccess;
            existing.AllowTinderAccess = item.AllowTinderAccess;
            existing.AllowAiPlanAccess = item.AllowAiPlanAccess;
            existing.AllowChatbotAccess = item.AllowChatbotAccess;
            existing.IsActive = item.IsActive;
        }

        await context.SaveChangesAsync();

        // New logic: Deactivate ANY User packages that are not in defaults
        var defaultUserTiers = defaults
            .Where(d => d.Audience == RoleConstants.User)
            .Select(d => d.Tier)
            .ToList();

        var legacyUserPackages = await context.ServicePackages
            .Where(p => p.Audience == RoleConstants.User && !defaultUserTiers.Contains(p.Tier) && p.IsActive)
            .ToListAsync();

        if (legacyUserPackages.Any())
        {
            foreach (var pkg in legacyUserPackages)
            {
                pkg.IsActive = false;
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task NormalizeLegacyTourPackageTiersAsync(AppDbContext context)
    {
        var legacyTierMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Basic"] = "TourBasic",
            ["Premium"] = "TourPlus"
        };
        var legacyTiers = legacyTierMap.Keys.ToArray();

        var legacyPackages = await context.ServicePackages
            .Where(p => p.Audience == RoleConstants.User && legacyTiers.Contains(p.Tier))
            .ToListAsync();

        if (!legacyPackages.Any())
        {
            return;
        }

        foreach (var legacy in legacyPackages)
        {
            if (!legacyTierMap.TryGetValue(legacy.Tier, out var targetTier))
            {
                continue;
            }

            var targetPackage = await context.ServicePackages
                .FirstOrDefaultAsync(p => p.Audience == RoleConstants.User && p.Tier == targetTier);

            if (targetPackage == null)
            {
                legacy.Tier = targetTier;
                legacy.Name = string.Equals(targetTier, "TourPlus", StringComparison.OrdinalIgnoreCase) ? "Tour Plus" : "Tour Basic";
                continue;
            }

            var relatedSubscriptions = await context.Subscriptions
                .Where(s => s.ServicePackageId == legacy.Id)
                .ToListAsync();

            foreach (var sub in relatedSubscriptions)
            {
                sub.ServicePackageId = targetPackage.Id;
            }

            legacy.IsActive = false;
            legacy.Tier = $"{legacy.Tier}Legacy";
            legacy.Name = $"{legacy.Name} (Legacy)";
        }

        await context.SaveChangesAsync();
    }

    private static IEnumerable<ServicePackage> GetDefaults()
    {
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ====== OWNER PACKAGES (unchanged) ======
        yield return new ServicePackage
        {
            Name = "Basic",
            Tier = "Basic",
            Audience = RoleConstants.Owner,
            MonthlyPrice = 99000,
            YearlyPrice = 990000,
            Description = "Goi co ban cho seller nho.",
            Features = "Hien thi tren ban do|Toi da 1 gian hang|1 Menu, khong gioi han mon|Ho tro qua email",
            MaxStores = 1,
            AllowAudioAccess = false,
            AllowTinderAccess = false,
            AllowAiPlanAccess = false,
            AllowChatbotAccess = false,
            IsActive = true,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "Premium",
            Tier = "Premium",
            Audience = RoleConstants.Owner,
            MonthlyPrice = 299000,
            YearlyPrice = 2990000,
            Description = "Goi mo rong cho seller dang tang truong.",
            Features = "Tat ca tinh nang Basic|Toi da 3 gian hang|Badge Premium tren app|Uu tien de xuat score +50|Thong ke nang cao|Ho tro uu tien",
            MaxStores = 3,
            AllowAudioAccess = false,
            AllowTinderAccess = false,
            AllowAiPlanAccess = false,
            AllowChatbotAccess = false,
            IsActive = true,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "VIP",
            Tier = "VIP",
            Audience = RoleConstants.Owner,
            MonthlyPrice = 599000,
            YearlyPrice = 5990000,
            Description = "Goi day du cho seller lon va chuoi gian hang.",
            Features = "Tat ca tinh nang Premium|Toi da 5 gian hang|Badge VIP tren app|Top de xuat score +100|Thong ke chi tiet|Quang cao tren banner|Ho tro rieng 24/7",
            MaxStores = 5,
            AllowAudioAccess = false,
            AllowTinderAccess = false,
            AllowAiPlanAccess = false,
            AllowChatbotAccess = false,
            IsActive = true,
            CreatedAt = createdAt
        };

        // ====== OLD USER PACKAGES (deactivated) ======
        yield return new ServicePackage
        {
            Name = "Audio Starter",
            Tier = "AudioBasic",
            Audience = RoleConstants.User,
            MonthlyPrice = 49000,
            YearlyPrice = 490000,
            Description = "Gói nghe thuyết minh cơ bản.",
            Features = "Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe",
            MaxStores = 0,
            AllowAudioAccess = true,
            AllowTinderAccess = false,
            AllowAiPlanAccess = false,
            AllowChatbotAccess = false,
            IsActive = false,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "Audio Plus",
            Tier = "AudioPremium",
            Audience = RoleConstants.User,
            MonthlyPrice = 99000,
            YearlyPrice = 990000,
            Description = "Gói nghe thuyết minh mở rộng.",
            Features = "Nghe thuyết minh 3 ngôn ngữ|Ưu tiên audio mới",
            MaxStores = 0,
            AllowAudioAccess = true,
            AllowTinderAccess = false,
            AllowAiPlanAccess = false,
            AllowChatbotAccess = false,
            IsActive = false,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "Audio Premium",
            Tier = "AudioVIP",
            Audience = RoleConstants.User,
            MonthlyPrice = 199000,
            YearlyPrice = 1990000,
            Description = "Gói audio cao cấp.",
            Features = "Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ ưu tiên",
            MaxStores = 0,
            AllowAudioAccess = true,
            AllowTinderAccess = false,
            AllowAiPlanAccess = false,
            AllowChatbotAccess = false,
            IsActive = false,
            CreatedAt = createdAt
        };

        // ====== NEW USER PACKAGES (Daily billing) ======
        yield return new ServicePackage
        {
            Name = "Tour Basic",
            Tier = "TourBasic",
            Audience = RoleConstants.User,
            MonthlyPrice = 50000,   // 50K/ngay
            YearlyPrice = 50000,
            Description = "Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng. Sử dụng trong 1 ngày.",
            Features = "Sử dụng trong 1 ngày|Tự động phát thuyết minh khi đến gần POI|Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe|Chatbot Thổ Địa tư vấn món ăn|!Tinder Ẩm Thực|!AI Kế Hoạch Tour",
            MaxStores = 0,
            AllowAudioAccess = true,
            AllowTinderAccess = false,
            AllowAiPlanAccess = false,
            AllowChatbotAccess = true,
            IsActive = true,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "Tour Plus",
            Tier = "TourPlus",
            Audience = RoleConstants.User,
            MonthlyPrice = 99000,   // 99K/ngay
            YearlyPrice = 99000,
            Description = "Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn. Sử dụng trong 1 ngày.",
            Features = "Sử dụng trong 1 ngày|Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Groq|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot",
            MaxStores = 0,
            AllowAudioAccess = true,
            AllowTinderAccess = true,
            AllowAiPlanAccess = true,
            AllowChatbotAccess = true,
            IsActive = true,
            CreatedAt = createdAt
        };
    }
}

