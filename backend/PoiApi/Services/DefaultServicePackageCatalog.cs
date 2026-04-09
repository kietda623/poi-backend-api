using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Services;

public static class DefaultServicePackageCatalog
{
    public static async Task SyncAsync(AppDbContext context)
    {
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
            existing.IsActive = true;
        }

        await context.SaveChangesAsync();
    }

    private static IEnumerable<ServicePackage> GetDefaults()
    {
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
            IsActive = true,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "Audio Ngay",
            Tier = "Basic",
            Audience = RoleConstants.User,
            MonthlyPrice = 20000,
            YearlyPrice = 20000,
            Description = "Goi audio theo ngay cho user can nghe nhanh.",
            Features = "Su dung trong 1 ngay|Nghe thuyet minh 3 ngon ngu|Phu hop cho chuyen di ngan",
            MaxStores = 0,
            AllowAudioAccess = true,
            IsActive = true,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "Audio Thang",
            Tier = "Premium",
            Audience = RoleConstants.User,
            MonthlyPrice = 100000,
            YearlyPrice = 100000,
            Description = "Goi audio theo thang cho user nghe thuong xuyen.",
            Features = "Su dung trong 1 thang|Nghe thuyet minh 3 ngon ngu|Khong gioi han luot nghe trong thoi gian goi",
            MaxStores = 0,
            AllowAudioAccess = true,
            IsActive = true,
            CreatedAt = createdAt
        };

        yield return new ServicePackage
        {
            Name = "Audio Nam",
            Tier = "VIP",
            Audience = RoleConstants.User,
            MonthlyPrice = 999000,
            YearlyPrice = 999000,
            Description = "Goi audio theo nam cho user su dung lau dai.",
            Features = "Su dung trong 1 nam|Nghe thuyet minh 3 ngon ngu|Tiet kiem chi phi cho nguoi nghe thuong xuyen",
            MaxStores = 0,
            AllowAudioAccess = true,
            IsActive = true,
            CreatedAt = createdAt
        };
    }
}
