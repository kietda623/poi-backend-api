namespace AppUser.Models;

public class AppServicePackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public bool AllowAudioAccess { get; set; }
    public string RecommendedBillingCycle { get; set; } = string.Empty;
    public decimal DisplayPrice { get; set; }
    public string DisplayLabel { get; set; } = string.Empty;
}

public class AppSubscriptionEnvelopeDto
{
    public bool HasSubscription { get; set; }
    public bool CanAccessAudio { get; set; }
    public AppCurrentSubscriptionDto? Subscription { get; set; }
}

public class AppCurrentSubscriptionDto
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
    public string? CheckoutUrl { get; set; }
    public long? PaymentOrderCode { get; set; }
    public string? PaymentLinkId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public bool AllowAudioAccess { get; set; }
}

public class AppCheckoutSubscriptionResultDto
{
    public string Message { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
    public string? QrCode { get; set; }
    public long OrderCode { get; set; }
}
