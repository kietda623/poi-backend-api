using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/usage-history")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class UsageHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsageHistoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUsageHistory()
        {
            var histories = _context.UsageHistories
                .Include(uh => uh.Shop)
                .OrderByDescending(uh => uh.ListenedAt)
                .Select(uh => new
                {
                    uh.Id,
                    uh.DeviceId,
                    uh.ShopId,
                    StoreName = uh.Shop != null ? uh.Shop.Name : "N/A",
                    uh.LanguageCode,
                    uh.ListenedAt,
                    uh.DurationSeconds
                })
                .ToList();

            return Ok(histories);
        }
    }
}
