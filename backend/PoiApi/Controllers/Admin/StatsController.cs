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

        public AdminStatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAdminStats([FromQuery] int? month, [FromQuery] int? year)
        {
            var currentYear = year ?? DateTime.UtcNow.Year;

            var subscriptionRevenueQuery = _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.ServicePackage)
                .Where(s => s.PaymentStatus == SubscriptionConstants.PaymentPaid)
                .Where(s => (s.ActivatedAt ?? s.CreatedAt).Year == currentYear);

            var listensQuery = _context.UsageHistories
                .Include(u => u.Shop)
                .Where(u => u.ListenedAt.Year == currentYear);

            if (month.HasValue)
            {
                subscriptionRevenueQuery = subscriptionRevenueQuery.Where(s => (s.ActivatedAt ?? s.CreatedAt).Month == month.Value);
                listensQuery = listensQuery.Where(u => u.ListenedAt.Month == month.Value);
            }

            var totalRevenue = subscriptionRevenueQuery.Sum(s => (decimal?)s.Price) ?? 0m;
            var totalListens = listensQuery.Count();

            var revenueByMonth = subscriptionRevenueQuery
                .GroupBy(s => (s.ActivatedAt ?? s.CreatedAt).Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(s => s.Price),
                    SubscriptionCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            var revenueByShop = subscriptionRevenueQuery
                .GroupBy(s => new { s.UserId, SellerName = s.User.FullName, s.User.Email })
                .Select(g => new
                {
                    ShopId = g.Key.UserId,
                    ShopName = string.IsNullOrWhiteSpace(g.Key.SellerName) ? g.Key.Email : g.Key.SellerName,
                    Revenue = g.Sum(s => s.Price),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var listensByMonth = listensQuery
                .GroupBy(u => u.ListenedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Listens = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            var listensByShop = listensQuery
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
                Year = currentYear,
                Month = month,
                TotalRevenue = totalRevenue,
                TotalListens = totalListens,
                RevenueByMonth = revenueByMonth,
                RevenueByShop = revenueByShop,
                ListensByMonth = listensByMonth,
                ListensByShop = listensByShop
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
