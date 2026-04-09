using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;
using PoiApi.Services;
using System.Globalization;

namespace PoiApi.Controllers.Owner
{
    [ApiController]
    [Route("api/stats/seller")]
    [Authorize(Roles = "ADMIN,OWNER")]
    public class SellerStatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SellerStatsController(AppDbContext context)
        {
            _context = context;
        }

        private static (DateTime Start, DateTime End, string Period) ResolvePeriodRange(string? period, DateTime anchorDate)
        {
            var normalized = string.IsNullOrWhiteSpace(period) ? "week" : period.Trim().ToLowerInvariant();
            var date = anchorDate.Date;

            return normalized switch
            {
                "month" => (new DateTime(date.Year, date.Month, 1), new DateTime(date.Year, date.Month, 1).AddMonths(1), "month"),
                "quarter" => ResolveQuarter(date),
                "year" => (new DateTime(date.Year, 1, 1), new DateTime(date.Year + 1, 1, 1), "year"),
                _ => ResolveWeek(date)
            };
        }

        private static (DateTime Start, DateTime End, string Period) ResolveWeek(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            var daysToMonday = dayOfWeek == DayOfWeek.Sunday ? -6 : -(int)dayOfWeek + 1;
            var start = date.AddDays(daysToMonday);
            return (start, start.AddDays(7), "week");
        }

        private static (DateTime Start, DateTime End, string Period) ResolveQuarter(DateTime date)
        {
            var quarter = ((date.Month - 1) / 3) + 1;
            var startMonth = ((quarter - 1) * 3) + 1;
            var start = new DateTime(date.Year, startMonth, 1);
            return (start, start.AddMonths(3), "quarter");
        }

        private static List<(DateTime Start, DateTime End, string Label)> BuildBuckets(string period, DateTime start, DateTime end)
        {
            var buckets = new List<(DateTime Start, DateTime End, string Label)>();

            if (period == "quarter" || period == "year")
            {
                for (var cursor = start; cursor < end; cursor = cursor.AddMonths(1))
                {
                    buckets.Add((cursor, cursor.AddMonths(1), $"Thang {cursor.Month}"));
                }
            }
            else
            {
                for (var cursor = start; cursor < end; cursor = cursor.AddDays(1))
                {
                    buckets.Add((cursor, cursor.AddDays(1), cursor.ToString("dd/MM")));
                }
            }

            return buckets;
        }

        [HttpGet("{sellerId}")]
        public IActionResult GetSellerStats(int sellerId, [FromQuery] int? storeId = null)
        {
            var query = _context.Shops.Where(s => s.OwnerId == sellerId);
            if (storeId.HasValue && storeId.Value > 0)
            {
                query = query.Where(s => s.Id == storeId.Value);
            }

            var shops = query.ToList();
            var shopIds = shops.Select(s => s.Id).ToList();

            if (!shopIds.Any())
            {
                return Ok(new List<object>());
            }

            var totalListens = _context.UsageHistories.Count(u => shopIds.Contains(u.ShopId));
            var totalReviews = _context.Reviews.Count(r => shopIds.Contains(r.ShopId));
            var totalRevenue = _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.ServicePackage.Audience == RoleConstants.User)
                .Where(s => s.PaymentStatus == SubscriptionConstants.PaymentPaid)
                .Where(s => s.RevenueRecipientUserId == sellerId || _context.UsageHistories.Any(u => u.DeviceId == "user:" + s.UserId && shopIds.Contains(u.ShopId)))
                .Where(s => !storeId.HasValue || s.RevenueRecipientShopId == storeId.Value)
                .Sum(s => (decimal?)s.Price) ?? 0m;

            return Ok(new List<object>
            {
                new
                {
                    TotalListens = totalListens,
                    TotalRevenue = totalRevenue,
                    TotalReviews = totalReviews,
                    Month = "Tất cả thời gian"
                }
            });
        }

        [HttpGet("/api/stores/{storeId}/reviews")]
        public IActionResult GetStoreReviews(int storeId, [FromQuery] int? sellerId = null)
        {
            var query = _context.Reviews.AsQueryable();
            if (storeId > 0)
            {
                query = query.Where(r => r.ShopId == storeId);
            }
            else if (sellerId.HasValue && sellerId.Value > 0)
            {
                var shopIds = _context.Shops.Where(s => s.OwnerId == sellerId.Value).Select(s => s.Id).ToList();
                query = query.Where(r => shopIds.Contains(r.ShopId));
            }
            else
            {
                return Ok(new List<object>());
            }

            var reviews = query
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .Select(r => new
                {
                    r.Id,
                    r.ShopId,
                    r.CustomerName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToList();

            return Ok(reviews);
        }

        [HttpGet("{sellerId}/revenue")]
        public IActionResult GetSellerRevenue(int sellerId, [FromQuery] string? week, [FromQuery] int? storeId = null)
        {
            DateTime targetDate;
            if (!string.IsNullOrEmpty(week) && DateTime.TryParse(week, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                targetDate = parsed;
            }
            else
            {
                targetDate = DateTime.UtcNow;
            }

            var dayOfWeek = targetDate.DayOfWeek;
            var daysToMonday = dayOfWeek == DayOfWeek.Sunday ? -6 : -(int)dayOfWeek + 1;
            var weekStart = targetDate.Date.AddDays(daysToMonday);
            var weekEnd = weekStart.AddDays(7);

            var q = _context.Shops.Where(s => s.OwnerId == sellerId);
            if (storeId.HasValue && storeId.Value > 0)
            {
                q = q.Where(s => s.Id == storeId.Value);
            }

            var shopIds = q.Select(s => s.Id).ToList();

            if (!shopIds.Any())
            {
                return Ok(new
                {
                    WeekStart = weekStart.ToString("yyyy-MM-dd"),
                    WeekEnd = weekEnd.AddDays(-1).ToString("yyyy-MM-dd"),
                    TotalRevenue = 0m,
                    DailyRevenue = new List<object>(),
                    ShopRevenue = new List<object>()
                });
            }

            var shopNameLookup = _context.Shops
                .Where(s => shopIds.Contains(s.Id))
                .ToDictionary(s => s.Id, s => s.Name);

            var subscriptionsInWeek = _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.ServicePackage.Audience == RoleConstants.User)
                .Where(s => s.PaymentStatus == SubscriptionConstants.PaymentPaid)
                .Where(s => s.RevenueRecipientUserId == sellerId || _context.UsageHistories.Any(u => u.DeviceId == "user:" + s.UserId && shopIds.Contains(u.ShopId)))
                .Where(s => !storeId.HasValue || s.RevenueRecipientShopId == storeId.Value)
                .Where(s => (s.ActivatedAt ?? s.CreatedAt) >= weekStart && (s.ActivatedAt ?? s.CreatedAt) < weekEnd)
                .ToList();

            var dailyRevenue = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var day = weekStart.AddDays(i);
                    var dayOrders = subscriptionsInWeek.Where(s => (s.ActivatedAt ?? s.CreatedAt).Date == day.Date);
                    return new
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        DayName = day.ToString("ddd", CultureInfo.InvariantCulture),
                        Revenue = dayOrders.Sum(s => s.Price),
                        OrderCount = dayOrders.Count()
                    };
                })
                .ToList();

            var shopRevenue = subscriptionsInWeek
                .Where(s => s.RevenueRecipientShopId.HasValue && shopNameLookup.ContainsKey(s.RevenueRecipientShopId.Value))
                .GroupBy(s => s.RevenueRecipientShopId!.Value)
                .Select(g => new
                {
                    ShopId = g.Key,
                    ShopName = shopNameLookup[g.Key],
                    Revenue = g.Sum(x => x.Price),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            return Ok(new
            {
                WeekStart = weekStart.ToString("yyyy-MM-dd"),
                WeekEnd = weekEnd.AddDays(-1).ToString("yyyy-MM-dd"),
                TotalRevenue = subscriptionsInWeek.Sum(s => s.Price),
                DailyRevenue = dailyRevenue,
                ShopRevenue = shopRevenue
            });
        }

        [HttpGet("{sellerId}/overview")]
        public IActionResult GetSellerOverview(int sellerId, [FromQuery] string? period = null, [FromQuery] string? anchorDate = null, [FromQuery] int? storeId = null)
        {
            var parsedAnchorDate = DateTime.TryParse(anchorDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var anchor)
                ? anchor
                : DateTime.UtcNow;

            var (startDate, endDate, normalizedPeriod) = ResolvePeriodRange(period, parsedAnchorDate);
            var buckets = BuildBuckets(normalizedPeriod, startDate, endDate);

            var shopQuery = _context.Shops.Where(s => s.OwnerId == sellerId);
            if (storeId.HasValue && storeId.Value > 0)
            {
                shopQuery = shopQuery.Where(s => s.Id == storeId.Value);
            }

            var shops = shopQuery.ToList();
            var shopIds = shops.Select(s => s.Id).ToList();

            if (!shopIds.Any())
            {
                return Ok(new
                {
                    Period = normalizedPeriod,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.AddDays(-1).ToString("yyyy-MM-dd"),
                    TotalRevenue = 0m,
                    TotalListens = 0,
                    RevenueBreakdown = new List<object>(),
                    ListenBreakdown = new List<object>(),
                    RevenueByShop = new List<object>(),
                    ListensByShop = new List<object>()
                });
            }

            var shopNameLookup = shops.ToDictionary(s => s.Id, s => s.Name);

            var subscriptions = _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Include(s => s.User)
                .Where(s => s.ServicePackage.Audience == RoleConstants.User)
                .Where(s => s.PaymentStatus == SubscriptionConstants.PaymentPaid)
                .Where(s => s.RevenueRecipientUserId == sellerId || _context.UsageHistories.Any(u => u.DeviceId == "user:" + s.UserId && shopIds.Contains(u.ShopId)))
                .Where(s => !storeId.HasValue || s.RevenueRecipientShopId == storeId.Value)
                .Where(s => (s.ActivatedAt ?? s.CreatedAt) >= startDate && (s.ActivatedAt ?? s.CreatedAt) < endDate)
                .ToList();

            var listens = _context.UsageHistories
                .Include(u => u.Shop)
                .Where(u => shopIds.Contains(u.ShopId))
                .Where(u => u.ListenedAt >= startDate && u.ListenedAt < endDate)
                .ToList();

            var revenueBreakdown = buckets
                .Select(bucket =>
                {
                    var bucketOrders = subscriptions.Where(s => (s.ActivatedAt ?? s.CreatedAt) >= bucket.Start && (s.ActivatedAt ?? s.CreatedAt) < bucket.End);
                    return new
                    {
                        Label = bucket.Label,
                        Revenue = bucketOrders.Sum(s => s.Price),
                        OrderCount = bucketOrders.Count()
                    };
                })
                .ToList();

            var listenBreakdown = buckets
                .Select(bucket => new
                {
                    Label = bucket.Label,
                    Listens = listens.Count(u => u.ListenedAt >= bucket.Start && u.ListenedAt < bucket.End)
                })
                .ToList();

            var revenueByShop = subscriptions
                .GroupBy(s => s.RevenueRecipientShopId)
                .Select(g => new
                {
                    ShopId = g.Key ?? 0,
                    ShopName = g.Key.HasValue && shopNameLookup.ContainsKey(g.Key.Value) 
                               ? shopNameLookup[g.Key.Value] 
                               : "Vãng lai / Hệ thống",
                    Revenue = g.Sum(x => x.Price),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var recentTransactions = subscriptions
                .OrderByDescending(s => s.ActivatedAt ?? s.CreatedAt)
                .Take(20)
                .Select(s => new
                {
                    s.Id,
                    CustomerName = s.User != null ? s.User.FullName : "Khách vãng lai",
                    PackageName = s.ServicePackage != null ? s.ServicePackage.Name : "Gói Audio",
                    Price = s.Price,
                    Date = s.ActivatedAt ?? s.CreatedAt,
                    Status = s.Status
                })
                .ToList();

            var listensByShop = listens
                .GroupBy(u => new { u.ShopId, ShopName = u.Shop != null ? u.Shop.Name : "Unknown Shop" })
                .Select(g => new
                {
                    ShopId = g.Key.ShopId,
                    ShopName = g.Key.ShopName,
                    Listens = g.Count()
                })
                .OrderByDescending(x => x.Listens)
                .ToList();

            return Ok(new
            {
                Period = normalizedPeriod,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.AddDays(-1).ToString("yyyy-MM-dd"),
                TotalRevenue = subscriptions.Sum(s => s.Price),
                TotalListens = listens.Count,
                RevenueBreakdown = revenueBreakdown,
                ListenBreakdown = listenBreakdown,
                RevenueByShop = revenueByShop,
                ListensByShop = listensByShop,
                RecentTransactions = recentTransactions
            });
        }
    }
}
