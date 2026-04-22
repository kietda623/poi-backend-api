namespace AppUser.Services
{
    public static class AppConfig
    {
        // Change this to your current HTTPS ngrok domain when testing on a real phone.
        // Example: "https://abc-123.ngrok-free.app"
        public static string NgrokBaseUrl = "https://sandpaper-pawing-alive.ngrok-free.dev";

        // Set true to force all requests through ngrok (required for physical devices).
        public static bool UseNgrokTunnel = true;

        public static string BaseDomain
        {
            get
            {
                if (UseNgrokTunnel && !string.IsNullOrWhiteSpace(NgrokBaseUrl))
                {
                    return NgrokBaseUrl.TrimEnd('/');
                }

                return DeviceInfo.Platform == DevicePlatform.Android
                    ? "http://10.0.2.2:5279"
                    : "http://localhost:5279";
            }
        }

        public static string BaseApiUrl => $"{BaseDomain}/api/";

        public static void ConfigureHttpClient(HttpClient client)
        {
            client.BaseAddress ??= new Uri(BaseDomain);

            if (!client.DefaultRequestHeaders.Contains("ngrok-skip-browser-warning"))
            {
                client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
            }
        }
        
        // Helper to resolve relative media URLs
        public static string ResolveUrl(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return string.Empty;
            if (relativeUrl.StartsWith("http")) return relativeUrl;
            
            return $"{BaseDomain}/{relativeUrl.TrimStart('/')}";
        }
    }
}
