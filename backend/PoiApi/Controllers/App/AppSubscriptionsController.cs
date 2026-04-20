using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;
using PoiApi.Models;
using PoiApi.Services;

[ApiController]
[Route("api/app/subscriptions")]
// Cho phép cả USER và GUEST truy cập endpoint subscriptions
[Authorize]
public class AppSubscriptionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PayOsService _payOsService;
    private readonly SubscriptionAccessService _subscriptionAccessService;
    private readonly ILogger<AppSubscriptionsController> _logger;

    public AppSubscriptionsController(
        AppDbContext context,
        PayOsService payOsService,
        SubscriptionAccessService subscriptionAccessService,
        ILogger<AppSubscriptionsController> logger)
    {
        _context = context;
        _payOsService = payOsService;
        _subscriptionAccessService = subscriptionAccessService;
        _logger = logger;
    }

    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages()
    {
        await DefaultServicePackageCatalog.SyncAsync(_context);
        var packages = await _context.ServicePackages
            .Where(p => p.IsActive && p.Audience == RoleConstants.User)
            .OrderBy(p => p.MonthlyPrice)
            .ToListAsync();

        var response = packages.Select(p => new
        {
            p.Id,
            p.Name,
            p.Tier,
            p.Audience,
            p.MonthlyPrice,
            p.YearlyPrice,
            p.Description,
            Features = p.Features.Split('|', StringSplitOptions.RemoveEmptyEntries),
            p.AllowAudioAccess,
            RecommendedBillingCycle = ResolveUserBillingCycle(p.Tier),
            DisplayPrice = ResolveDisplayPrice(p.Tier, p.MonthlyPrice, p.YearlyPrice),
            DisplayLabel = ResolveDisplayLabel(p.Tier)
        }).ToList();

        return Ok(response);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMySubscription()
    {
        // Hỗ trợ cả User và Guest
        if (GuestTokenService.IsGuest(User))
        {
            var deviceId = GuestTokenService.GetDeviceId(User) ?? "";
            return Ok(await BuildCurrentSubscriptionEnvelopeByDeviceAsync(deviceId));
        }

        var userId = GetUserId();
        await MarkExpiredSubscriptionsAsync(userId);
        return Ok(await BuildCurrentSubscriptionEnvelopeAsync(userId));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        await MarkExpiredSubscriptionsAsync(userId);

        var subs = await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.User)
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
                s.CreatedAt,
                s.ActivatedAt
            })
            .ToListAsync();

        return Ok(subs);
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe(AppSubscribeDto dto)
    {
        var isGuest = GuestTokenService.IsGuest(User);
        var userId = isGuest ? 0 : GetUserId();
        var guestDeviceId = isGuest ? (GuestTokenService.GetDeviceId(User) ?? "") : null;

        var package = await _context.ServicePackages.FirstOrDefaultAsync(p => p.Id == dto.PackageId && p.Audience == RoleConstants.User);
        if (package == null)
        {
            return BadRequest(new { message = "Gói không tồn tại." });
        }

        var billingCycle = ResolveRequestedBillingCycle(dto.BillingCycle, package.Tier);
        if (billingCycle == null)
        {
            return BadRequest(new { message = "Billing cycle is invalid. Only Daily, Monthly or Yearly are supported." });
        }

        if (!package.IsActive)
        {
            return BadRequest(new { message = "This audio package is inactive." });
        }

        // Kiểm tra existing: Guest dùng DeviceId, User dùng UserId
        Subscription? existing;
        if (isGuest)
        {
            existing = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.DeviceId == guestDeviceId && s.UserId == null && s.ServicePackage.Audience == RoleConstants.User)
                .Where(s => s.Status == SubscriptionConstants.Active || s.Status == SubscriptionConstants.PendingPayment || s.Status == SubscriptionConstants.Pending)
                .FirstOrDefaultAsync();
        }
        else
        {
            existing = await _context.Subscriptions
                .Include(s => s.ServicePackage)
                .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.User)
                .Where(s => s.Status == SubscriptionConstants.Active || s.Status == SubscriptionConstants.PendingPayment || s.Status == SubscriptionConstants.Pending)
                .FirstOrDefaultAsync();
        }

        bool isUpgrade = false;
        if (existing != null)
        {
            // Allow upgrade from TourBasic to TourPlus
            if (existing.Status == SubscriptionConstants.Active && existing.ServicePackage.Tier == "TourBasic" && package.Tier == "TourPlus")
            {
                isUpgrade = true;
            }
            else
            {
                return BadRequest(new { message = "Bạn đã có gói đang hoạt động hoặc chờ thanh toán." });
            }
        }

        var now = DateTime.UtcNow;
        var price = ResolvePackagePrice(package, billingCycle);
        
        // Special logic for upgrade as per requested
        if (isUpgrade)
        {
            price = 50000; // Fixed upgrade price requested by user
        }

        if (price <= 0)
        {
            return BadRequest(new { message = "Audio package price is invalid." });
        }

        // Resolve revenue: Guest dùng DeviceId, User dùng UserId
        (int ShopId, int OwnerId)? revenueRecipient;
        if (isGuest)
        {
            revenueRecipient = await ResolveRevenueRecipientByDeviceAsync(guestDeviceId!);
        }
        else
        {
            revenueRecipient = await ResolveRevenueRecipientAsync(userId);
        }

        var orderCode = BuildOrderCode(isGuest ? guestDeviceId!.GetHashCode() : userId);
        var endDate = _subscriptionAccessService.CalculateEndDate(now, billingCycle);

        var subscription = new Subscription
        {
            UserId = isGuest ? null : userId,
            DeviceId = isGuest ? guestDeviceId : null,
            GuestEmail = isGuest ? dto.GuestEmail : null,
            ServicePackageId = dto.PackageId,
            BillingCycle = billingCycle,
            Price = price,
            StartDate = now,
            EndDate = endDate,
            Status = SubscriptionConstants.PendingPayment,
            PaymentStatus = SubscriptionConstants.PaymentPending,
            PaymentProvider = "PayOS",
            PaymentOrderCode = orderCode,
            RevenueRecipientUserId = revenueRecipient?.OwnerId,
            RevenueRecipientShopId = revenueRecipient?.ShopId
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        try
        {
            var paymentLink = await _payOsService.CreatePaymentLinkAsync(new PayOsCreatePaymentRequest
            {
                OrderCode = orderCode,
                Amount = decimal.ToInt32(decimal.Round(price, MidpointRounding.AwayFromZero)),
                Description = TruncateDescription($"APP {package.Tier} {subscription.Id}"),
                ReturnUrl = _payOsService.BuildReturnUrl(RoleConstants.User, subscription.Id),
                CancelUrl = _payOsService.BuildCancelUrl(RoleConstants.User, subscription.Id),
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
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.ServicePackage.Audience == RoleConstants.User);

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

            var effectiveStatus = _subscriptionAccessService.ResolveEffectivePayOsState(
                paymentInfo.Status,
                paymentInfo.Amount,
                paymentInfo.AmountPaid);
            _logger.LogInformation(
                "Sync payment for app subscription {SubscriptionId}: rawStatus={RawStatus}, amount={Amount}, amountPaid={AmountPaid}, effectiveStatus={EffectiveStatus}",
                sub.Id,
                paymentInfo.Status,
                paymentInfo.Amount,
                paymentInfo.AmountPaid,
                effectiveStatus);
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
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = GetUserId();
        var sub = await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && s.ServicePackage.Audience == RoleConstants.User);

        if (sub == null)
        {
            return NotFound(new { message = "Subscription not found." });
        }

        if (sub.Status != SubscriptionConstants.Active && sub.Status != SubscriptionConstants.PendingPayment && sub.Status != SubscriptionConstants.Pending)
        {
            return BadRequest(new { message = "Only active or pending subscriptions can be cancelled." });
        }

        sub.Status = SubscriptionConstants.Cancelled;
        sub.PaymentStatus = sub.PaymentStatus == SubscriptionConstants.PaymentPaid
            ? sub.PaymentStatus
            : SubscriptionConstants.PaymentCancelled;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Subscription cancelled." });
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task MarkExpiredSubscriptionsAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var expired = await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.User)
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
        var sub = await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .Where(s => s.UserId == userId && s.ServicePackage.Audience == RoleConstants.User)
            .Where(s => s.Status == SubscriptionConstants.Active ||
                        s.Status == SubscriptionConstants.PendingPayment ||
                        s.Status == SubscriptionConstants.Pending)
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
                s.CheckoutUrl,
                s.PaymentOrderCode,
                s.PaymentLinkId,
                s.CreatedAt,
                s.ActivatedAt,
                s.ServicePackage.AllowAudioAccess
            })
            .FirstOrDefaultAsync();

        if (sub == null)
        {
            return new { hasSubscription = false, canAccessAudio = false };
        }

        var canAccessAudio = string.Equals(sub.Status, SubscriptionConstants.Active, StringComparison.OrdinalIgnoreCase) &&
                             sub.EndDate > DateTime.UtcNow &&
                             sub.AllowAudioAccess;

        return new { hasSubscription = true, canAccessAudio, subscription = sub };
    }

    private static long BuildOrderCode(int userId)
    {
        var seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return long.Parse($"{seconds}{userId % 1000:D3}");
    }

    private static string? NormalizeBillingCycle(string? billingCycle)
    {
        if (string.Equals(billingCycle, "Daily", StringComparison.OrdinalIgnoreCase))
        {
            return "Daily";
        }

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

    private static string TruncateDescription(string value)
    {
        return value.Length <= 25 ? value : value[..25];
    }

    private static string? ResolveRequestedBillingCycle(string? requestedBillingCycle, string packageTier)
    {
        var recommended = ResolveUserBillingCycle(packageTier);
        if (!string.IsNullOrWhiteSpace(recommended))
        {
            return recommended;
        }

        return NormalizeBillingCycle(requestedBillingCycle);
    }

    private static string ResolveUserBillingCycle(string packageTier) => packageTier switch
    {
        "Basic" => "Daily",
        "Premium" => "Monthly",
        "VIP" => "Yearly",
        "TourBasic" => "Daily",
        "TourPlus" => "Daily",
        _ => "Monthly"
    };

    private static string ResolveDisplayLabel(string packageTier) => packageTier switch
    {
        "Basic" => "Theo ngay",
        "Premium" => "Theo thang",
        "VIP" => "Theo nam",
        "TourBasic" => "Theo ngay",
        "TourPlus" => "Theo ngay",
        _ => "Theo thang"
    };

    private static decimal ResolveDisplayPrice(string packageTier, decimal monthlyPrice, decimal yearlyPrice) => packageTier switch
    {
        "VIP" => yearlyPrice > 0 ? yearlyPrice : monthlyPrice,
        "TourBasic" => monthlyPrice,  // Daily price stored in MonthlyPrice
        "TourPlus" => monthlyPrice,   // Daily price stored in MonthlyPrice
        _ => monthlyPrice
    };

    private static decimal ResolvePackagePrice(ServicePackage package, string billingCycle)
    {
        if (string.Equals(billingCycle, "Yearly", StringComparison.OrdinalIgnoreCase))
        {
            return package.YearlyPrice;
        }

        return package.MonthlyPrice;
    }

    private async Task<(int ShopId, int OwnerId)?> ResolveRevenueRecipientAsync(int userId)
    {
        var latestUsage = await _context.UsageHistories
            .Where(x => x.DeviceId == $"user:{userId}")
            .Join(
                _context.Shops,
                usage => usage.ShopId,
                shop => shop.Id,
                (usage, shop) => new
                {
                    usage.ListenedAt,
                    usage.ShopId,
                    shop.OwnerId
                })
            .OrderByDescending(x => x.ListenedAt)
            .FirstOrDefaultAsync();

        if (latestUsage == null)
        {
            return null;
        }

        return (latestUsage.ShopId, latestUsage.OwnerId);
    }

    // === GUEST HELPERS ===

    // Lấy subscription hiện tại của Guest dựa trên DeviceId
    private async Task<object> BuildCurrentSubscriptionEnvelopeByDeviceAsync(string deviceId)
    {
        var sub = await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .Where(s => s.DeviceId == deviceId && s.UserId == null && s.ServicePackage.Audience == RoleConstants.User)
            .Where(s => s.Status == SubscriptionConstants.Active ||
                        s.Status == SubscriptionConstants.PendingPayment ||
                        s.Status == SubscriptionConstants.Pending)
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
                s.CheckoutUrl,
                s.PaymentOrderCode,
                s.PaymentLinkId,
                s.CreatedAt,
                s.ActivatedAt,
                s.ServicePackage.AllowAudioAccess
            })
            .FirstOrDefaultAsync();

        if (sub == null)
        {
            return new { hasSubscription = false, canAccessAudio = false };
        }

        var canAccessAudio = string.Equals(sub.Status, SubscriptionConstants.Active, StringComparison.OrdinalIgnoreCase) &&
                             sub.EndDate > DateTime.UtcNow &&
                             sub.AllowAudioAccess;

        return new { hasSubscription = true, canAccessAudio, subscription = sub };
    }

    // Tìm shop gần nhất mà Guest đã nghe dựa trên DeviceId
    private async Task<(int ShopId, int OwnerId)?> ResolveRevenueRecipientByDeviceAsync(string deviceId)
    {
        var latestUsage = await _context.UsageHistories
            .Where(x => x.DeviceId == deviceId)
            .Join(
                _context.Shops,
                usage => usage.ShopId,
                shop => shop.Id,
                (usage, shop) => new
                {
                    usage.ListenedAt,
                    usage.ShopId,
                    shop.OwnerId
                })
            .OrderByDescending(x => x.ListenedAt)
            .FirstOrDefaultAsync();

        if (latestUsage == null)
        {
            return null;
        }

        return (latestUsage.ShopId, latestUsage.OwnerId);
    }
}
