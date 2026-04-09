namespace foodstreet_admin.Models;

public class ServicePackageModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tier { get; set; } = "Basic";
    public string Audience { get; set; } = "OWNER";
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public int MaxStores { get; set; } = 1;
    public bool AllowAudioAccess { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SubscriptionModel
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public int ServicePackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageTier { get; set; } = "Basic";
    public string PackageAudience { get; set; } = "OWNER";
    public string BillingCycle { get; set; } = "Monthly";
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string PaymentProvider { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public long? PaymentOrderCode { get; set; }
    public string? CheckoutUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelRequestedAt { get; set; }
    public int? RevenueRecipientUserId { get; set; }
    public int? RevenueRecipientShopId { get; set; }
}

public class CheckoutSubscriptionResultModel
{
    public string Message { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageTier { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxStores { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
    public string? QrCode { get; set; }
    public long OrderCode { get; set; }
}

public class CurrentSubscriptionEnvelopeModel
{
    public bool HasSubscription { get; set; }
    public bool CanAccessAudio { get; set; }
    public bool CanCreateStore { get; set; }
    public int StoresUsed { get; set; }
    public int StoresRemaining { get; set; }
    public CurrentSubscriptionModel? Subscription { get; set; }
}

public class CurrentSubscriptionModel
{
    public int Id { get; set; }
    public int ServicePackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string PackageTier { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentProvider { get; set; } = string.Empty;
    public string? CheckoutUrl { get; set; }
    public long? PaymentOrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public int MaxStores { get; set; }
    public bool AllowAudioAccess { get; set; }
    public string PackageAudience { get; set; } = string.Empty;
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelRequestedAt { get; set; }
}
