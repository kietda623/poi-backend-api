using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/service-packages")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class ServicePackagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServicePackagesController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET all packages ────────────────────────────────────────
        [HttpGet]
        public IActionResult GetAll()
        {
            var packages = _context.ServicePackages
                .OrderBy(p => p.MonthlyPrice)
                .Select(p => new
                {
                    p.Id, p.Name, p.Tier,
                    p.MonthlyPrice, p.YearlyPrice,
                    p.Description,
                    Features = p.Features.Split('|', System.StringSplitOptions.RemoveEmptyEntries),
                    p.MaxStores, p.IsActive, p.CreatedAt
                })
                .ToList();

            return Ok(packages);
        }

        // ── GET single package ──────────────────────────────────────
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var p = _context.ServicePackages.Find(id);
            if (p == null) return NotFound();

            return Ok(new
            {
                p.Id, p.Name, p.Tier,
                p.MonthlyPrice, p.YearlyPrice,
                p.Description,
                Features = p.Features.Split('|', System.StringSplitOptions.RemoveEmptyEntries),
                p.MaxStores, p.IsActive, p.CreatedAt
            });
        }

        // ── POST create package ─────────────────────────────────────
        [HttpPost]
        public IActionResult Create(CreateServicePackageDto dto)
        {
            var package = new ServicePackage
            {
                Name = dto.Name,
                Tier = dto.Tier,
                MonthlyPrice = dto.MonthlyPrice,
                YearlyPrice = dto.YearlyPrice,
                Description = dto.Description,
                Features = dto.Features,
                MaxStores = dto.MaxStores,
                IsActive = dto.IsActive
            };

            _context.ServicePackages.Add(package);
            _context.SaveChanges();

            return Ok(new { message = "Tạo gói thành công", packageId = package.Id });
        }

        // ── PUT update package ──────────────────────────────────────
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateServicePackageDto dto)
        {
            var package = _context.ServicePackages.Find(id);
            if (package == null) return NotFound();

            package.Name = dto.Name;
            package.Tier = dto.Tier;
            package.MonthlyPrice = dto.MonthlyPrice;
            package.YearlyPrice = dto.YearlyPrice;
            package.Description = dto.Description;
            package.Features = dto.Features;
            package.MaxStores = dto.MaxStores;
            package.IsActive = dto.IsActive;

            _context.SaveChanges();
            return Ok(new { message = "Cập nhật gói thành công" });
        }

        // ── DELETE package ──────────────────────────────────────────
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var package = _context.ServicePackages.Find(id);
            if (package == null) return NotFound();

            _context.ServicePackages.Remove(package);
            _context.SaveChanges();
            return NoContent();
        }

        // ── GET all subscriptions (Admin view) ──────────────────────
        [HttpGet("subscriptions")]
        public IActionResult GetSubscriptions()
        {
            var subs = _context.Subscriptions
                .Include(s => s.User)
                .Include(s => s.ServicePackage)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new
                {
                    s.Id,
                    SellerId = s.UserId,
                    SellerName = s.User.FullName,
                    SellerEmail = s.User.Email,
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
                .ToList();

            return Ok(subs);
        }

        // ── PATCH approve subscription ──────────────────────────────
        [HttpPatch("subscriptions/{id}/approve")]
        public IActionResult ApproveSubscription(int id)
        {
            var sub = _context.Subscriptions.Find(id);
            if (sub == null) return NotFound();

            sub.Status = "Active";
            _context.SaveChanges();
            return Ok(new { message = "Đã duyệt đăng ký" });
        }

        // ── PATCH cancel subscription ───────────────────────────────
        [HttpPatch("subscriptions/{id}/cancel")]
        public IActionResult CancelSubscription(int id)
        {
            var sub = _context.Subscriptions.Find(id);
            if (sub == null) return NotFound();

            sub.Status = "Cancelled";
            _context.SaveChanges();
            return Ok(new { message = "Đã hủy đăng ký" });
        }
    }
}
