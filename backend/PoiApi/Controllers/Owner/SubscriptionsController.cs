using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Owner;
using PoiApi.Models;
using System.Security.Claims;

namespace PoiApi.Controllers.Owner
{
    [ApiController]
    [Route("api/owner/subscriptions")]
    [Authorize(Roles = RoleConstants.Owner)]
    public class SubscriptionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubscriptionsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── GET available packages ──────────────────────────────────
        [HttpGet("packages")]
        public IActionResult GetAvailablePackages()
        {
            var packages = _context.ServicePackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.MonthlyPrice)
                .Select(p => new
                {
                    p.Id, p.Name, p.Tier,
                    p.MonthlyPrice, p.YearlyPrice,
                    p.Description,
                    Features = p.Features.Split('|', System.StringSplitOptions.RemoveEmptyEntries),
                    p.MaxStores
                })
                .ToList();

            return Ok(packages);
        }

        // ── GET my subscription ─────────────────────────────────────
        [HttpGet("my")]
        public IActionResult GetMySubscription()
        {
            var userId = GetUserId();

            var sub = _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    s.ServicePackageId,
                    PackageName = s.ServicePackage.Name,
                    PackageTier = s.ServicePackage.Tier,
                    s.BillingCycle,
                    s.Price,
                    s.StartDate,
                    s.EndDate,
                    s.Status,
                    s.CreatedAt
                })
                .FirstOrDefault();

            if (sub == null)
                return Ok(new { hasSubscription = false });

            return Ok(new { hasSubscription = true, subscription = sub });
        }

        // ── GET my subscription history ─────────────────────────────
        [HttpGet("history")]
        public IActionResult GetMyHistory()
        {
            var userId = GetUserId();

            var subs = _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    PackageName = s.ServicePackage.Name,
                    PackageTier = s.ServicePackage.Tier,
                    s.BillingCycle,
                    s.Price,
                    s.StartDate,
                    s.EndDate,
                    s.Status,
                    s.CreatedAt
                })
                .ToList();

            return Ok(subs);
        }

        // ── POST subscribe to a package ─────────────────────────────
        [HttpPost]
        public IActionResult Subscribe(SubscribeDto dto)
        {
            var userId = GetUserId();

            var package = _context.ServicePackages.Find(dto.PackageId);
            if (package == null)
                return BadRequest("Gói dịch vụ không tồn tại");

            if (!package.IsActive)
                return BadRequest("Gói dịch vụ đã ngưng hoạt động");

            // Kiểm tra xem seller có đang có gói Active/Pending không
            var existingActive = _context.Subscriptions
                .FirstOrDefault(s => s.UserId == userId &&
                    (s.Status == "Active" || s.Status == "Pending"));

            if (existingActive != null)
                return BadRequest("Bạn đang có gói dịch vụ đang hoạt động hoặc chờ duyệt. Vui lòng hủy trước khi đăng ký gói mới.");

            var now = DateTime.UtcNow;
            var price = dto.BillingCycle == "Yearly" ? package.YearlyPrice : package.MonthlyPrice;
            var endDate = dto.BillingCycle == "Yearly" ? now.AddYears(1) : now.AddMonths(1);

            var subscription = new Subscription
            {
                UserId = userId,
                ServicePackageId = dto.PackageId,
                BillingCycle = dto.BillingCycle,
                Price = price,
                StartDate = now,
                EndDate = endDate,
                Status = "Pending"
            };

            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Đăng ký gói thành công! Đang chờ Admin duyệt.",
                subscriptionId = subscription.Id
            });
        }

        // ── DELETE cancel my subscription ───────────────────────────
        [HttpDelete("{id}")]
        public IActionResult CancelMySubscription(int id)
        {
            var userId = GetUserId();

            var sub = _context.Subscriptions
                .FirstOrDefault(s => s.Id == id && s.UserId == userId);

            if (sub == null)
                return NotFound("Không tìm thấy đăng ký");

            if (sub.Status != "Active" && sub.Status != "Pending")
                return BadRequest("Chỉ có thể hủy đăng ký đang hoạt động hoặc chờ duyệt");

            sub.Status = "Cancelled";
            _context.SaveChanges();

            return Ok(new { message = "Đã hủy đăng ký gói dịch vụ" });
        }
    }
}
