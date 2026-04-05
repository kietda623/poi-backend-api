namespace AppUser.Services
{
    public static class AppConfig
    {
        // Change this if your API is running on a different address
        public static string BaseApiUrl = "http://localhost:5279/api/";
        
        // Helper to resolve relative media URLs
        public static string ResolveUrl(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return string.Empty;
            if (relativeUrl.StartsWith("http")) return relativeUrl;
            
            var baseDomain = BaseApiUrl.Replace("/api/", "");
            return $"{baseDomain.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        }
    }
}
