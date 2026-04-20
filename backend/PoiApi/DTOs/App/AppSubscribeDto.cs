namespace PoiApi.DTOs.App;

public class AppSubscribeDto
{
    public int PackageId { get; set; }
    public string BillingCycle { get; set; } = "Monthly";

    // Email tùy chọn khi Guest mua gói (để khôi phục nếu cài lại app)
    public string? GuestEmail { get; set; }
}
