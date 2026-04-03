namespace PoiApi.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;   // vi, en, ko, ja
        public string Name { get; set; } = string.Empty;   // Tiếng Việt, English…
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
