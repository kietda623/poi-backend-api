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
        public ICollection<Menu>? Menus { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
