namespace PoiApi.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ServicePackageId { get; set; }
        public ServicePackage ServicePackage { get; set; } = null!;
        public string BillingCycle { get; set; } = "Monthly";
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentProvider { get; set; } = "PayOS";
        public string PaymentStatus { get; set; } = "Pending";
        public long? PaymentOrderCode { get; set; }
        public string? PaymentLinkId { get; set; }
        public string? CheckoutUrl { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public bool CancelAtPeriodEnd { get; set; }
        public DateTime? CancelRequestedAt { get; set; }
        public int? RevenueRecipientUserId { get; set; }
        public int? RevenueRecipientShopId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
