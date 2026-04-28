using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Hubs;
using PoiApi.Models;
using PoiApi.Services;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/stats")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class AdminStatsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminStatsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAdminStats([FromQuery] int? month, [FromQuery] int? year)
        {
            var currentYear = year ?? DateTime.UtcNow.Year;
            var userSharePercent = _configuration.GetValue<int>("PlatformRevenue:UserSharePercent", 20);

            // === Subscriptions base query (only Paid) ===
            var paidSubsQuery = _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.ServicePackage)
                .Where(s => s.PaymentStatus == SubscriptionConstants.PaymentPaid)
                .Where(s => (s.ActivatedAt ?? s.CreatedAt).Year == currentYear);

            if (month.HasValue)
            {
                paidSubsQuery = paidSubsQuery.Where(s => (s.ActivatedAt ?? s.CreatedAt).Month == month.Value);
            }

            var paidSubs = paidSubsQuery.ToList();

            // === Split by Audience ===
            var sellerSubs = paidSubs.Where(s => s.ServicePackage.Audience == RoleConstants.Owner).ToList();
            var userSubs = paidSubs.Where(s => s.ServicePackage.Audience == RoleConstants.User).ToList();

            // === Summary ===
            var sellerRevenue = sellerSubs.Sum(s => s.Price);
            var userRevenueTotal = userSubs.Sum(s => s.Price);
            var platformShare = Math.Round(userRevenueTotal * userSharePercent / 100m, 0);
            var totalRevenue = sellerRevenue + platformShare;

            // Active subscriptions count (regardless of year filter)
            var activeSubsCount = _context.Subscriptions
                .Count(s => s.Status == SubscriptionConstants.Active);

            var newThisMonth = _context.Subscriptions
                .Where(s => s.PaymentStatus == SubscriptionConstants.PaymentPaid)
                .Where(s => (s.ActivatedAt ?? s.CreatedAt).Year == DateTime.UtcNow.Year)
                .Where(s => (s.ActivatedAt ?? s.CreatedAt).Month == DateTime.UtcNow.Month)
                .Count();

            // === Seller stats by tier ===
            var sellerByTier = sellerSubs
                .GroupBy(s => s.ServicePackage.Tier)
                .Select(g => new
                {
                    Tier = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.Price)
                })
                .OrderBy(x => x.Revenue)
                .ToList();

            // === Seller stats by seller ===
            var sellerBySeller = sellerSubs
                .GroupBy(s => new { s.UserId, s.User?.FullName, s.User?.Email })
                .Select(g => new
                {
                    SellerId = g.Key.UserId,
                    SellerName = string.IsNullOrWhiteSpace(g.Key.FullName) ? g.Key.Email : g.Key.FullName,
                    Email = g.Key.Email ?? "",
                    PackageName = g.OrderByDescending(x => x.Price).First().ServicePackage.Name,
                    Revenue = g.Sum(s => s.Price),
                    SubscriptionCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // === User stats by tier ===
            var userByTier = userSubs
                .GroupBy(s => s.ServicePackage.Name)
                .Select(g => new
                {
                    Tier = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => s.Price)
                })
                .OrderBy(x => x.Revenue)
                .ToList();

            // === Revenue by month (split) ===
            var revenueByMonth = paidSubs
                .GroupBy(s => (s.ActivatedAt ?? s.CreatedAt).Month)
                .Select(g => new
                {
                    Month = g.Key,
                    SellerRevenue = g.Where(s => s.ServicePackage.Audience == RoleConstants.Owner).Sum(s => s.Price),
                    UserRevenue = Math.Round(
                        g.Where(s => s.ServicePackage.Audience == RoleConstants.User).Sum(s => s.Price) * userSharePercent / 100m, 0),
                    UserRevenueTotal = g.Where(s => s.ServicePackage.Audience == RoleConstants.User).Sum(s => s.Price),
                    TotalRevenue = g.Where(s => s.ServicePackage.Audience == RoleConstants.Owner).Sum(s => s.Price)
                        + Math.Round(g.Where(s => s.ServicePackage.Audience == RoleConstants.User).Sum(s => s.Price) * userSharePercent / 100m, 0),
                    SubscriptionCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // === Listens stats ===
            var listensQuery = _context.UsageHistories
                .Include(u => u.Shop)
                .Where(u => u.ListenedAt.Year == currentYear);

            if (month.HasValue)
            {
                listensQuery = listensQuery.Where(u => u.ListenedAt.Month == month.Value);
            }

            var totalListens = listensQuery.Count();
            var guestListens = listensQuery.Count(u => u.GuestId != null && u.GuestId != "");
            var registeredListens = totalListens - guestListens;

            var listensByMonth = listensQuery
                .GroupBy(u => u.ListenedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Listens = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // === Legacy compatibility: RevenueByShop ===
            var revenueByShop = sellerBySeller.Select(s => new
            {
                ShopId = s.SellerId,
                ShopName = s.SellerName,
                Revenue = s.Revenue,
                OrderCount = s.SubscriptionCount
            }).ToList();

            return Ok(new
            {
                Year = currentYear,
                Month = month,
                Summary = new
                {
                    TotalRevenue = totalRevenue,
                    SellerRevenue = sellerRevenue,
                    UserRevenueTotal = userRevenueTotal,
                    PlatformShare = platformShare,
                    UserSharePercent = userSharePercent,
                    TotalSubscriptions = paidSubs.Count,
                    ActiveSubscriptions = activeSubsCount,
                    NewSubscriptionsThisMonth = newThisMonth
                },
                SellerStats = new
                {
                    TotalRevenue = sellerRevenue,
                    SubscriptionCount = sellerSubs.Count,
                    ByTier = sellerByTier,
                    BySeller = sellerBySeller
                },
                UserStats = new
                {
                    TotalRevenue = userRevenueTotal,
                    PlatformSharePercent = userSharePercent,
                    PlatformShareAmount = platformShare,
                    SubscriptionCount = userSubs.Count,
                    ByTier = userByTier
                },
                RevenueByMonth = revenueByMonth,
                TotalListens = totalListens,
                GuestListens = guestListens,
                RegisteredListens = registeredListens,
                ListensByMonth = listensByMonth,
                // Legacy fields for backward compatibility
                RevenueByShop = revenueByShop
            });
        }

        [HttpGet("revenue")]
        public IActionResult GetRevenueStats([FromQuery] int? month, [FromQuery] int? year)
        {
            return GetAdminStats(month, year);
        }

        [HttpGet("online-count")]
        public IActionResult GetOnlineCount()
        {
            return Ok(new { count = AppPresenceHub.GetOnlineCount() });
        }
    }
}
