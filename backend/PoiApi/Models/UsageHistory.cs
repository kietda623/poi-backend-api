namespace PoiApi.Models
{
    public class UsageHistory
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = string.Empty;   // device id hoặc email user
        public int ShopId { get; set; }
        public Shop? Shop { get; set; }
        public string LanguageCode { get; set; } = "vi";
        public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
        public int DurationSeconds { get; set; }
    }
}
