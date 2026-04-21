namespace PoiApi.Hubs;

public class OnlineUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ServicePackage { get; set; } = "Basic";
    public DateTime ConnectedAtUtc { get; set; }
}
