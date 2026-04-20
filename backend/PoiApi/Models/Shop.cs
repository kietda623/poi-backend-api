namespace PoiApi.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public int? PoiId { get; set; }
        public POI? Poi { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public ICollection<Menu>? Menus { get; set; }
        public bool IsActive { get; set; } = true;
        public int ViewCount { get; set; } = 0;
        public int ListenCount { get; set; } = 0;
        public double AverageRating { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // QR Code URL - auto-generated when shop is created
        public string? QrCodeUrl { get; set; }
    }
}
