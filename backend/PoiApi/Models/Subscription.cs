namespace PoiApi.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        // UserId nullable: Guest không cần tài khoản, dùng DeviceId thay thế
        public int? UserId { get; set; }
        public User? User { get; set; }

        // DeviceId để liên kết gói dịch vụ với thiết bị của Guest
        public string? DeviceId { get; set; }

        // Email thu thập tại bước thanh toán (cho Guest khôi phục gói)
        public string? GuestEmail { get; set; }

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
