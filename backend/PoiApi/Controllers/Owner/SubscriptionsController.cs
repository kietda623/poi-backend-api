using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Owner;
using PoiApi.Models;
using PoiApi.Services;
using System.Security.Claims;

namespace PoiApi.Controllers.Owner
{
    [ApiController]
    [Route("api/owner/subscriptions")]
    [Authorize(Roles = RoleConstants.Owner)]
    public class SubscriptionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PayOsService _payOsService;
        private readonly SubscriptionAccessService _subscriptionAccessService;

        public SubscriptionsController(
            AppDbContext context,
            PayOsService payOsService,
            SubscriptionAccessService subscriptionAccessService)
        {
            _context = context;
            _payOsService = payOsService;
            _subscriptionAccessService = subscriptionAccessService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("packages")]
        public async Task<IActionResult> GetAvailablePackages()
        {
            await DefaultServicePackageCatalog.SyncAsync(_context);
            var packages = _context.ServicePackages
                .Where(p => p.IsActive && p.Audience == RoleConstants.Owner)
                .OrderBy(p => p.MonthlyPrice)
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
                    p.AllowAudioAccess
                })
                .ToList();

            return Ok(packages);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMySubscription()
        {
            var userId = GetUserId();
            await MarkExpiredSubscriptionsAsync(userId);
            return Ok(await BuildCurrentSubscriptionEnvelopeAsync(userId));
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMyHistory()
        {
            var userId = GetUserId();
            await MarkExpiredSubscriptionsAsync(userId);

            var subs = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.Owner)
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
                    s.PaymentStatus,
                    s.PaymentProvider,
                    s.CreatedAt,
                    s.ActivatedAt
                })
                .ToListAsync();

            return Ok(subs);
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(SubscribeDto dto)
        {
            var userId = GetUserId();
            await MarkExpiredSubscriptionsAsync(userId);
            var billingCycle = NormalizeBillingCycle(dto.BillingCycle);
            if (billingCycle == null)
            {
                return BadRequest(new { message = "Billing cycle is invalid. Only Monthly or Yearly are supported." });
            }

            var package = await _context.ServicePackages.FirstOrDefaultAsync(p => p.Id == dto.PackageId && p.Audience == RoleConstants.Owner);
            if (package == null)
            {
                return BadRequest(new { message = "Service package not found." });
            }

            if (!package.IsActive)
            {
                return BadRequest(new { message = "This service package is inactive." });
            }

            var existingPending = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.Owner)
                .Where(s => s.Status == SubscriptionConstants.PendingPayment || s.Status == SubscriptionConstants.Pending)
                .FirstOrDefaultAsync();

            if (existingPending != null)
            {
                return BadRequest(new { message = "You already have a pending seller package payment." });
            }

            var currentActive = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.Owner)
                .Where(s => s.Status == SubscriptionConstants.Active && s.EndDate > DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (currentActive != null)
            {
                var currentRank = GetTierRank(currentActive.ServicePackage.Tier);
                var targetRank = GetTierRank(package.Tier);
                if (targetRank <= currentRank)
                {
                    return BadRequest(new { message = "Seller chi co the nang cap len goi cao hon. Khong the chuyen ve goi thap hon hoac dang ky lai goi hien tai." });
                }
            }

            var now = DateTime.UtcNow;
            var price = string.Equals(billingCycle, "Yearly", StringComparison.OrdinalIgnoreCase)
                ? package.YearlyPrice
                : package.MonthlyPrice;
            if (price <= 0)
            {
                return BadRequest(new { message = "Service package price is invalid." });
            }

            var orderCode = BuildOrderCode(userId);
            var endDate = _subscriptionAccessService.CalculateEndDate(now, billingCycle);

            var subscription = new Subscription
            {
                UserId = userId,
                ServicePackageId = dto.PackageId,
                BillingCycle = billingCycle,
                Price = price,
                StartDate = now,
                EndDate = endDate,
                Status = SubscriptionConstants.PendingPayment,
                PaymentStatus = SubscriptionConstants.PaymentPending,
                PaymentProvider = "PayOS",
                PaymentOrderCode = orderCode,
                RevenueRecipientUserId = null,
                RevenueRecipientShopId = null
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            try
            {
                var paymentLink = await _payOsService.CreatePaymentLinkAsync(new PayOsCreatePaymentRequest
                {
                    OrderCode = orderCode,
                    Amount = decimal.ToInt32(decimal.Round(price, MidpointRounding.AwayFromZero)),
                    Description = TruncateDescription($"OWNER {package.Tier} {subscription.Id}"),
                    ReturnUrl = _payOsService.BuildSellerPackageCallbackUrl(subscription.Id),
                    CancelUrl = _payOsService.BuildSellerPackageCallbackUrl(subscription.Id),
                    ExpiredAt = new DateTimeOffset(now.AddMinutes(30)).ToUnixTimeSeconds(),
                    Items = new List<PayOsPaymentItem>
                    {
                        new()
                        {
                            Name = package.Name,
                            Quantity = 1,
                            Price = decimal.ToInt32(decimal.Round(price, MidpointRounding.AwayFromZero))
                        }
                    }
                });

                subscription.PaymentLinkId = paymentLink.PaymentLinkId;
                subscription.CheckoutUrl = paymentLink.CheckoutUrl;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Payment link created successfully.",
                    subscriptionId = subscription.Id,
                    packageId = package.Id,
                    packageName = package.Name,
                    packageTier = package.Tier,
                    billingCycle,
                    price,
                    maxStores = package.MaxStores,
                    checkoutUrl = paymentLink.CheckoutUrl,
                    paymentLinkId = paymentLink.PaymentLinkId,
                    qrCode = paymentLink.QrCode,
                    orderCode
                });
            }
            catch (Exception ex)
            {
                subscription.Status = SubscriptionConstants.Cancelled;
                subscription.PaymentStatus = SubscriptionConstants.PaymentFailed;
                await _context.SaveChangesAsync();
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/sync-payment")]
        public async Task<IActionResult> SyncMySubscriptionPayment(int id)
        {
            var userId = GetUserId();
            var sub = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.ServicePackage.Audience == RoleConstants.Owner);

            if (sub == null)
            {
                return NotFound(new { message = "Subscription not found." });
            }

            if (!string.Equals(sub.PaymentProvider, "PayOS", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Only PayOS subscriptions can be synced." });
            }

            var paymentReference = !string.IsNullOrWhiteSpace(sub.PaymentLinkId)
                ? sub.PaymentLinkId
                : sub.PaymentOrderCode?.ToString();

            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                return BadRequest(new { message = "Subscription does not have a PayOS payment reference." });
            }

            try
            {
                var paymentInfo = await _payOsService.GetPaymentLinkInfoAsync(paymentReference);
                if (!string.IsNullOrWhiteSpace(paymentInfo.Id))
                {
                    sub.PaymentLinkId = paymentInfo.Id;
                }

                var effectiveStatus = ResolvePaymentStatus(paymentInfo.Status, paymentInfo.Amount, paymentInfo.AmountPaid);
                await _subscriptionAccessService.ApplyPayOsPaymentStateAsync(sub, effectiveStatus);
                await _context.SaveChangesAsync();
                await MarkExpiredSubscriptionsAsync(userId);

                return Ok(await BuildCurrentSubscriptionEnvelopeAsync(userId));
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelMySubscription(int id)
        {
            var userId = GetUserId();
            var sub = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.ServicePackage.Audience == RoleConstants.Owner);

            if (sub == null)
            {
                return NotFound(new { message = "Subscription not found." });
            }

            if (sub.Status != SubscriptionConstants.Active && sub.Status != SubscriptionConstants.PendingPayment && sub.Status != SubscriptionConstants.Pending)
            {
                return BadRequest(new { message = "Only active or pending subscriptions can be cancelled." });
            }

            if (sub.Status == SubscriptionConstants.Active && string.Equals(sub.PaymentStatus, SubscriptionConstants.PaymentPaid, StringComparison.OrdinalIgnoreCase))
            {
                sub.CancelAtPeriodEnd = true;
                sub.CancelRequestedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Auto-renew has been cancelled. Your current seller package will stay active until the end of this billing cycle." });
            }

            sub.Status = SubscriptionConstants.Cancelled;
            sub.PaymentStatus = sub.PaymentStatus == SubscriptionConstants.PaymentPaid
                ? sub.PaymentStatus
                : SubscriptionConstants.PaymentCancelled;
            sub.CancelAtPeriodEnd = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Subscription cancelled." });
        }

        private async Task MarkExpiredSubscriptionsAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var expired = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.Owner)
                .Where(s => s.Status == SubscriptionConstants.Active && s.EndDate <= now)
                .ToListAsync();

            if (!expired.Any())
            {
                return;
            }

            foreach (var item in expired)
            {
                item.Status = SubscriptionConstants.Expired;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<object> BuildCurrentSubscriptionEnvelopeAsync(int userId)
        {
            var currentStores = await _context.Shops.CountAsync(s => s.OwnerId == userId);

            var sub = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.Owner)
                .Where(s => s.Status == SubscriptionConstants.Active ||
                            s.Status == SubscriptionConstants.PendingPayment ||
                            s.Status == SubscriptionConstants.Pending)
                .OrderByDescending(s => s.Status == SubscriptionConstants.Active ? 1 : 0)
                .ThenByDescending(s => s.EndDate)
                .ThenByDescending(s => s.CreatedAt)
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
                    s.PaymentStatus,
                    s.PaymentProvider,
                    s.CheckoutUrl,
                    s.PaymentOrderCode,
                    s.PaymentLinkId,
                    s.CreatedAt,
                    s.ActivatedAt,
                    s.CancelAtPeriodEnd,
                    s.CancelRequestedAt,
                    s.ServicePackage.MaxStores,
                    s.ServicePackage.AllowAudioAccess,
                    PackageAudience = s.ServicePackage.Audience
                })
                .FirstOrDefaultAsync();

            if (sub == null)
            {
                return new { hasSubscription = false };
            }

            return new
            {
                hasSubscription = true,
                canCreateStore = sub.Status == SubscriptionConstants.Active && currentStores < sub.MaxStores,
                storesUsed = currentStores,
                storesRemaining = Math.Max(sub.MaxStores - currentStores, 0),
                subscription = sub
            };
        }

        private static string? ResolvePaymentStatus(string? paymentStatus, int amount, int amountPaid)
        {
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                return paymentStatus;
            }

            if (amount > 0 && amountPaid >= amount)
            {
                return "PAID";
            }

            return null;
        }

        private static long BuildOrderCode(int userId)
        {
            var seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return long.Parse($"{seconds}{userId % 1000:D3}");
        }

        private static string TruncateDescription(string value)
        {
            return value.Length <= 25 ? value : value[..25];
        }

        private static string? NormalizeBillingCycle(string? billingCycle)
        {
            if (string.Equals(billingCycle, "Monthly", StringComparison.OrdinalIgnoreCase))
            {
                return "Monthly";
            }

            if (string.Equals(billingCycle, "Yearly", StringComparison.OrdinalIgnoreCase))
            {
                return "Yearly";
            }

            return null;
        }

        private static int GetTierRank(string? tier) => tier?.Trim().ToUpperInvariant() switch
        {
            "BASIC" => 1,
            "PREMIUM" => 2,
            "VIP" => 3,
            _ => 0
        };
    }
}
