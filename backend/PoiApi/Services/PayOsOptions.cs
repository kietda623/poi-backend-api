namespace PoiApi.Services;

public class PayOsOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChecksumKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-merchant.payos.vn";
    public string ReturnUrl { get; set; } = "http://localhost:5279/payment/success";
    public string CancelUrl { get; set; } = "http://localhost:5279/payment/cancel";
}
