using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

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
        public IActionResult GetAdminStats()
        {
            var totalStores = _context.Shops.Count();
            var pendingStores = _context.Shops.Count(s => !s.IsActive);
            
            // Total customers that have role == USER (Id = -3)
            var customerRole = _context.Roles.FirstOrDefault(r => r.Name == RoleConstants.User);
            var totalCustomers = customerRole != null ? _context.Users.Count(u => u.RoleId == customerRole.Id) : 0;
            
            // Mocking these since we don't have deep UsageHistory and Revenue tracking running yet
            var totalListens = _context.UsageHistories.Count();
            var revenue = 15400000; 

            var topStores = _context.Shops
                .Select(s => new {
                    s.Name,
                    Category = "Chưa rõ",
                    Listens = 0 // Mocked for now
                })
                .Take(5)
                .ToList();

            var langStats = _context.Languages
                .Select(l => new {
                    Language = l.Name,
                    Listens = 0,
                    Percent = 0.0
                })
                .ToList();

            var monthlyOverview = new List<object>
            {
                new { Month = "Tháng hiện tại", NewStores = totalStores, NewCustomers = totalCustomers, TotalListens = totalListens, Revenue = revenue, Growth = 0.0 }
            };

            return Ok(new
            {
                TotalStores = totalStores,
                PendingStores = pendingStores,
                TotalCustomers = totalCustomers,
                TotalListens = totalListens,
                TotalRevenue = revenue,
                TopStores = topStores,
                LangStats = langStats,
                MonthlyOverview = monthlyOverview
            });
        }
    }
}
