namespace PoiApi.Services;

/// <summary>Subscription info returned to MAUI client for permission gating</summary>
public class UserSubscriptionInfo
{
    public string Tier { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public bool AllowAudio { get; set; }
    public bool AllowTinder { get; set; }
    public bool AllowAiPlan { get; set; }
    public bool AllowChatbot { get; set; }
}
