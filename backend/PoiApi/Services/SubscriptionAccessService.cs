using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Services;

public class SubscriptionAccessService
{
    private readonly AppDbContext _context;

    public SubscriptionAccessService(AppDbContext context)
    {
        _context = context;
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
                s.ServicePackage.Audience == audience &&
                s.ServicePackage.IsActive)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasAudioAccessAsync(int userId)
    {
        var sub = await GetActiveSubscriptionAsync(userId, RoleConstants.User);
        return sub?.ServicePackage.AllowAudioAccess == true;
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
        var normalized = paymentState?.Trim().ToUpperInvariant();

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

        subscription.PaymentStatus = SubscriptionConstants.PaymentFailed;
        subscription.Status = SubscriptionConstants.Cancelled;
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
