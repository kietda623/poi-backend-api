namespace foodstreet_admin.Models;

public class UsageHistoryModel
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = ""; // or User Email if logged in
    public int StoreId { get; set; }
    public string StoreName { get; set; } = "";
    public string LanguageCode { get; set; } = "vi";
    public DateTime ListenedAt { get; set; } = DateTime.Now;
    public int DurationSeconds { get; set; }
}
