using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/shops")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class ShopsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShopsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/shops
        [HttpGet]
        public IActionResult GetAllShops()
        {
            var shops = _context.Shops
                .Include(s => s.Owner)
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.Address,
                    Phone = "", // Add as needed if User table eventually gets Phone
                    ImageUrl = s.Poi != null ? s.Poi.ImageUrl : "",
                    AudioUrl = "", // Schema does not have AudioUrl yet
                    Category = "Mặc định", // Có thể mở rộng sau nếu có Category thực
                    Status = s.IsActive ? "Active" : "Pending", // Tạm dùng IsActive
                    SellerId = s.OwnerId,
                    SellerName = s.Owner.FullName,
                    s.CreatedAt,
                    Latitude = 0.0,
                    Longitude = 0.0,
                    TotalListens = 0,
                    TotalViews = 0,
                    Rating = 0.0
                })
                .ToList();

            return Ok(shops);
        }

        // PATCH: api/admin/shops/{id}/approve
        [HttpPatch("{id}/approve")]
        public IActionResult ApproveShop(int id)
        {
            var shop = _context.Shops.Find(id);
            if (shop == null) return NotFound("Gian hàng không tồn tại");

            shop.IsActive = true;
            _context.SaveChanges();

            return Ok(new { message = "Gian hàng đã được duyệt" });
        }

        // PATCH: api/admin/shops/{id}/reject
        [HttpPatch("{id}/reject")]
        public IActionResult RejectShop(int id)
        {
            var shop = _context.Shops.Find(id);
            if (shop == null) return NotFound("Gian hàng không tồn tại");

            shop.IsActive = false;
            _context.SaveChanges();

            return Ok(new { message = "Gian hàng đã bị từ chối/vô hiệu hóa" });
        }

        // DELETE: api/admin/shops/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteShop(int id)
        {
            var shop = _context.Shops.Find(id);
            if (shop == null) return NotFound("Gian hàng không tồn tại");

            _context.Shops.Remove(shop);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
