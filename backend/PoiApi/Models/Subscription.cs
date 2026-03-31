namespace PoiApi.Models
{
    /// <summary>Đăng ký gói dịch vụ của Seller (Owner)</summary>
    public class Subscription
    {
        public int Id { get; set; }

        // FK → User (Seller / Owner)
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // FK → ServicePackage
        public int ServicePackageId { get; set; }
        public ServicePackage ServicePackage { get; set; } = null!;

        /// <summary>Monthly | Yearly</summary>
        public string BillingCycle { get; set; } = "Monthly";
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Pending | Active | Expired | Cancelled</summary>
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
