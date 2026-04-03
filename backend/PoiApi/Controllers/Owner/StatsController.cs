using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoiApi.Data;

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

        [HttpGet("{sellerId}")]
        public IActionResult GetSellerStats(int sellerId)
        {
            // Verify if the current user is an owner, they must match sellerId
            // A simple placeholder implementation:
            return Ok(new List<object>()); // Returning empty list for now as per requirement details
        }
    }
}
