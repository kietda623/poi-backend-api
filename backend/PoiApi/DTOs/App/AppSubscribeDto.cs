namespace PoiApi.DTOs.App;

public class AppSubscribeDto
{
    public int PackageId { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
}
