namespace PoiApi.Models
{
    /// <summary>Gói dịch vụ (Basic / Premium / VIP)</summary>
    public class ServicePackage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>Basic | Premium | VIP</summary>
        public string Tier { get; set; } = "Basic";
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        /// <summary>Tính năng, phân cách bằng dấu |</summary>
        public string Features { get; set; } = string.Empty;
        public int MaxStores { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
