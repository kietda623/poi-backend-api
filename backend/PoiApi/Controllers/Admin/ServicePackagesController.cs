using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.Models;
using PoiApi.Services;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await DefaultServicePackageCatalog.SyncAsync(_context);
            var packages = _context.ServicePackages
                .OrderBy(p => p.Audience)
                .ThenBy(p => p.MonthlyPrice)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Tier,
                    p.Audience,
                    p.MonthlyPrice,
                    p.YearlyPrice,
                    p.Description,
                    Features = p.Features.Split('|', StringSplitOptions.RemoveEmptyEntries),
                    p.MaxStores,
                    p.AllowAudioAccess,
                    p.IsActive,
                    p.CreatedAt
                })
                .ToList();

            return Ok(packages);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var package = _context.ServicePackages.Find(id);
            if (package == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                package.Id,
                package.Name,
                package.Tier,
                package.Audience,
                package.MonthlyPrice,
                package.YearlyPrice,
                package.Description,
                Features = package.Features.Split('|', StringSplitOptions.RemoveEmptyEntries),
                package.MaxStores,
                package.AllowAudioAccess,
                package.IsActive,
                package.CreatedAt
            });
        }

        [HttpPost]
        public IActionResult Create(CreateServicePackageDto dto)
        {
            var package = new ServicePackage
            {
                Name = dto.Name,
                Tier = dto.Tier,
                Audience = dto.Audience,
                MonthlyPrice = dto.MonthlyPrice,
                YearlyPrice = dto.YearlyPrice,
                Description = dto.Description,
                Features = dto.Features,
                MaxStores = dto.MaxStores,
                AllowAudioAccess = dto.AllowAudioAccess,
                IsActive = dto.IsActive
            };

            _context.ServicePackages.Add(package);
            _context.SaveChanges();

            return Ok(new { message = "Service package created successfully.", packageId = package.Id });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateServicePackageDto dto)
        {
            var package = _context.ServicePackages.Find(id);
            if (package == null)
            {
                return NotFound();
            }

            package.Name = dto.Name;
            package.Tier = dto.Tier;
            package.Audience = dto.Audience;
            package.MonthlyPrice = dto.MonthlyPrice;
            package.YearlyPrice = dto.YearlyPrice;
            package.Description = dto.Description;
            package.Features = dto.Features;
            package.MaxStores = dto.MaxStores;
            package.AllowAudioAccess = dto.AllowAudioAccess;
            package.IsActive = dto.IsActive;

            _context.SaveChanges();
            return Ok(new { message = "Service package updated successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var package = _context.ServicePackages.Find(id);
            if (package == null)
            {
                return NotFound();
            }

            _context.ServicePackages.Remove(package);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            var now = DateTime.UtcNow;
            var expired = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionConstants.Active && s.EndDate <= now)
                .ToListAsync();
            foreach (var item in expired)
            {
                item.Status = SubscriptionConstants.Expired;
            }
            if (expired.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            var subs = await _context.Subscriptions
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
                    PackageAudience = s.ServicePackage.Audience,
                    s.BillingCycle,
                    s.Price,
                    s.StartDate,
                    s.EndDate,
                    s.Status,
                    s.PaymentProvider,
                    s.PaymentStatus,
                    s.PaymentOrderCode,
                    s.CheckoutUrl,
                    s.CancelAtPeriodEnd,
                    s.CancelRequestedAt,
                    s.RevenueRecipientUserId,
                    s.RevenueRecipientShopId,
                    s.CreatedAt,
                    s.ActivatedAt
                })
                .ToListAsync();

            return Ok(subs);
        }

        [HttpPatch("subscriptions/{id}/approve")]
        public IActionResult ApproveSubscription(int id)
        {
            var sub = _context.Subscriptions.Find(id);
            if (sub == null)
            {
                return NotFound();
            }

            sub.Status = SubscriptionConstants.Active;
            sub.PaymentStatus = SubscriptionConstants.PaymentPaid;
            sub.ActivatedAt ??= DateTime.UtcNow;
            _context.SaveChanges();
            return Ok(new { message = "Subscription activated successfully." });
        }

        [HttpPatch("subscriptions/{id}/cancel")]
        public IActionResult CancelSubscription(int id)
        {
            var sub = _context.Subscriptions.Find(id);
            if (sub == null)
            {
                return NotFound();
            }

            sub.Status = SubscriptionConstants.Cancelled;
            if (sub.PaymentStatus != SubscriptionConstants.PaymentPaid)
            {
                sub.PaymentStatus = SubscriptionConstants.PaymentCancelled;
            }
            _context.SaveChanges();
            return Ok(new { message = "Subscription cancelled successfully." });
        }
    }
}
