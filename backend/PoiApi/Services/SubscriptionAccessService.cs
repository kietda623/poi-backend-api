using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Services;

public class SubscriptionAccessService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubscriptionAccessService> _logger;

    public SubscriptionAccessService(AppDbContext context, ILogger<SubscriptionAccessService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Subscription?> GetActiveSubscriptionAsync(int userId, string audience)
    {
        var now = DateTime.UtcNow;
        return await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .Where(s =>
                s.UserId == userId &&
                s.Status == SubscriptionConstants.Active &&
                s.EndDate > now &&
                s.ServicePackage.Audience == audience)
            .OrderByDescending(s => s.ActivatedAt.HasValue)
            .ThenByDescending(s => s.ActivatedAt)
            .ThenByDescending(s => s.EndDate)
            .ThenByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // === CÁC METHOD MỚI CHO GUEST (DeviceId-based) ===

    /// <summary>
    /// Tìm subscription đang hoạt động dựa trên DeviceId (cho Guest).
    /// </summary>
    public async Task<Subscription?> GetActiveSubscriptionByDeviceAsync(string deviceId, string audience)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return null;

        var now = DateTime.UtcNow;
        return await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .Where(s =>
                s.DeviceId == deviceId &&
                s.UserId == null && // Chỉ tìm subscription của Guest (không có UserId)
                s.Status == SubscriptionConstants.Active &&
                s.EndDate > now &&
                s.ServicePackage.Audience == audience)
            .OrderByDescending(s => s.ActivatedAt.HasValue)
            .ThenByDescending(s => s.ActivatedAt)
            .ThenByDescending(s => s.EndDate)
            .ThenByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Kiểm tra Guest có quyền nghe audio dựa trên DeviceId.
    /// </summary>
    public async Task<bool> HasAudioAccessByDeviceAsync(string deviceId)
    {
        var sub = await GetActiveSubscriptionByDeviceAsync(deviceId, RoleConstants.User);
        return sub?.ServicePackage.AllowAudioAccess == true;
    }

    /// <summary>
    /// Lấy thông tin subscription của Guest dựa trên DeviceId.
    /// </summary>
    public async Task<UserSubscriptionInfo?> GetSubscriptionInfoByDeviceAsync(string deviceId)
    {
        var sub = await GetActiveSubscriptionByDeviceAsync(deviceId, RoleConstants.User);
        if (sub == null) return null;
        return new UserSubscriptionInfo
        {
            Tier = sub.ServicePackage.Tier,
            PackageName = sub.ServicePackage.Name,
            EndDate = sub.EndDate,
            AllowAudio = sub.ServicePackage.AllowAudioAccess,
            AllowTinder = sub.ServicePackage.AllowTinderAccess,
            AllowAiPlan = sub.ServicePackage.AllowAiPlanAccess,
            AllowChatbot = sub.ServicePackage.AllowChatbotAccess
        };
    }

    // === END GUEST METHODS ===

    public async Task<bool> HasAudioAccessAsync(int userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId, RoleConstants.User);
        return sub?.ServicePackage.AllowAudioAccess == true;
    }

    /// <summary>Check if user has Tour Plus (Tinder feature)</summary>
    public async Task<bool> HasTinderAccessAsync(int userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId, RoleConstants.User);
        return sub?.ServicePackage.AllowTinderAccess == true;
    }

    public async Task<bool> HasTinderAccessByDeviceAsync(string deviceId)
    {
        var sub = await GetActiveSubscriptionByDeviceAsync(deviceId, RoleConstants.User);
        return sub?.ServicePackage.AllowTinderAccess == true;
    }

    /// <summary>Check if user has Tour Plus (AI Tour Plan feature)</summary>
    public async Task<bool> HasAiPlanAccessAsync(int userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId, RoleConstants.User);
        return sub?.ServicePackage.AllowAiPlanAccess == true;
    }

    public async Task<bool> HasAiPlanAccessByDeviceAsync(string deviceId)
    {
        var sub = await GetActiveSubscriptionByDeviceAsync(deviceId, RoleConstants.User);
        return sub?.ServicePackage.AllowAiPlanAccess == true;
    }

    /// <summary>Check if user has Tour Plus (Chatbot feature)</summary>
    public async Task<bool> HasChatbotAccessAsync(int userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId, RoleConstants.User);
        return sub?.ServicePackage.AllowChatbotAccess == true;
    }

    public async Task<bool> HasChatbotAccessByDeviceAsync(string deviceId)
    {
        var sub = await GetActiveSubscriptionByDeviceAsync(deviceId, RoleConstants.User);
        return sub?.ServicePackage.AllowChatbotAccess == true;
    }

    /// <summary>Get current subscription tier for the user (null if no active sub)</summary>
    public async Task<UserSubscriptionInfo?> GetUserSubscriptionInfoAsync(int userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId, RoleConstants.User);
        if (sub == null) return null;
        return new UserSubscriptionInfo
        {
            Tier = sub.ServicePackage.Tier,
            PackageName = sub.ServicePackage.Name,
            EndDate = sub.EndDate,
            AllowAudio = sub.ServicePackage.AllowAudioAccess,
            AllowTinder = sub.ServicePackage.AllowTinderAccess,
            AllowAiPlan = sub.ServicePackage.AllowAiPlanAccess,
            AllowChatbot = sub.ServicePackage.AllowChatbotAccess
        };
    }

    public DateTime CalculateEndDate(DateTime startDate, string billingCycle)
    {
        if (string.Equals(billingCycle, "Daily", StringComparison.OrdinalIgnoreCase))
        {
            return startDate.AddDays(1);
        }

        return string.Equals(billingCycle, "Yearly", StringComparison.OrdinalIgnoreCase)
            ? startDate.AddYears(1)
            : startDate.AddMonths(1);
    }

    public async Task ApplyPayOsPaymentStateAsync(Subscription subscription, string? paymentState)
    {
        var normalized = NormalizePayOsPaymentState(paymentState);

        if (string.Equals(normalized, "PAID", StringComparison.Ordinal))
        {
            subscription.PaymentStatus = SubscriptionConstants.PaymentPaid;
            subscription.Status = SubscriptionConstants.Active;
            subscription.ActivatedAt ??= DateTime.UtcNow;
            subscription.StartDate = subscription.ActivatedAt.Value;
            subscription.EndDate = CalculateEndDate(subscription.StartDate, subscription.BillingCycle);
            subscription.CancelAtPeriodEnd = false;
            subscription.CancelRequestedAt = null;
            await CloseOtherSubscriptionsAsync(subscription);
            return;
        }

        if (string.Equals(normalized, "FAILED", StringComparison.Ordinal))
        {
            subscription.PaymentStatus = SubscriptionConstants.PaymentFailed;
            subscription.Status = SubscriptionConstants.Cancelled;
            return;
        }

        if (string.Equals(normalized, "CANCELLED", StringComparison.Ordinal))
        {
            subscription.PaymentStatus = SubscriptionConstants.PaymentCancelled;
            subscription.Status = SubscriptionConstants.Cancelled;
            return;
        }

        if (string.Equals(normalized, "PENDING", StringComparison.Ordinal) ||
            string.Equals(normalized, "PROCESSING", StringComparison.Ordinal))
        {
            if (!string.Equals(subscription.PaymentStatus, SubscriptionConstants.PaymentPaid, StringComparison.OrdinalIgnoreCase))
            {
                subscription.PaymentStatus = SubscriptionConstants.PaymentPending;
            }

            subscription.Status = SubscriptionConstants.PendingPayment;
            return;
        }

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        if (string.Equals(subscription.PaymentStatus, SubscriptionConstants.PaymentPaid, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Ignoring unknown PayOS status for paid subscription {SubscriptionId}: {PaymentState}", subscription.Id, paymentState);
            return;
        }

        subscription.PaymentStatus = SubscriptionConstants.PaymentPending;
        subscription.Status = SubscriptionConstants.PendingPayment;
        _logger.LogWarning("Fallback unknown PayOS status to pending for subscription {SubscriptionId}: {PaymentState}", subscription.Id, paymentState);
    }

    public string ResolveEffectivePayOsState(string? paymentStatus, int amount, int amountPaid, bool successHint = false)
    {
        if (successHint || (amount > 0 && amountPaid >= amount))
        {
            return "PAID";
        }

        var normalized = NormalizePayOsPaymentState(paymentStatus);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            return normalized;
        }

        return "PENDING";
    }

    private static string NormalizePayOsPaymentState(string? paymentState)
    {
        if (string.IsNullOrWhiteSpace(paymentState))
        {
            return string.Empty;
        }

        var normalized = paymentState.Trim().ToUpperInvariant()
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);

        return normalized switch
        {
            "PAID" or "SUCCESS" or "SUCCEEDED" or "COMPLETED" or "COMPLETE" or "DONE" => "PAID",
            "PENDING" or "PROCESSING" or "INPROGRESS" or "WAITING" or "CREATED" => "PENDING",
            "CANCELLED" or "CANCELED" => "CANCELLED",
            "FAILED" or "FAIL" or "ERROR" or "EXPIRED" => "FAILED",
            _ => normalized
        };
    }

    private async Task CloseOtherSubscriptionsAsync(Subscription subscription)
    {
        var package = subscription.ServicePackage;
        if (package == null)
        {
            package = await _context.ServicePackages.FirstOrDefaultAsync(p => p.Id == subscription.ServicePackageId);
            if (package == null)
            {
                return;
            }
        }

        var relatedSubscriptions = await _context.Subscriptions
            .Include(s => s.ServicePackage)
            .Where(s => s.Id != subscription.Id)
            .Where(s => s.UserId == subscription.UserId)
            .Where(s => s.ServicePackage.Audience == package.Audience)
            .Where(s => s.Status == SubscriptionConstants.Active ||
                        s.Status == SubscriptionConstants.Pending ||
                        s.Status == SubscriptionConstants.PendingPayment)
            .ToListAsync();

        foreach (var item in relatedSubscriptions)
        {
            item.Status = SubscriptionConstants.Cancelled;
            if (!string.Equals(item.PaymentStatus, SubscriptionConstants.PaymentPaid, StringComparison.OrdinalIgnoreCase))
            {
                item.PaymentStatus = SubscriptionConstants.PaymentCancelled;
            }

            item.EndDate = subscription.StartDate;
            item.CancelAtPeriodEnd = false;
        }
    }
}
