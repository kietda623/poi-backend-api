namespace PoiApi.Models
{
    public class ServicePackage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = "Basic";
        public string Audience { get; set; } = RoleConstants.Owner;
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Features { get; set; } = string.Empty;
        public int MaxStores { get; set; } = 1;
        public bool AllowAudioAccess { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
