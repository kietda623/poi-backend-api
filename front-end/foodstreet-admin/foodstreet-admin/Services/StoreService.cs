using System.Text.Json;
using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class StoreService
{
    private readonly ApiService _api;
    private readonly ILogger<StoreService> _logger;

    public StoreService(ApiService api, ILogger<StoreService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<string?> UploadMediaAsync(Stream stream, string fileName)
    {
        var response = await _api.PostMultipartAsync<JsonElement>("media/upload-image", stream, fileName);
        if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("url", out var urlProp))
        {
            return urlProp.GetString();
        }
        return null;
    }

    public async Task<List<StoreModel>> GetStoresAsync(int? sellerId = null)
    {
        if (sellerId.HasValue)
        {
            var myStores = await _api.GetAsync<List<StoreModel>>("owner/shops");
            if (myStores != null) return myStores;
        }

        var stores = await _api.GetAsync<List<StoreModel>>("admin/shops") ?? new();
        if (sellerId.HasValue)
            return stores.Where(s => s.SellerId == sellerId.Value).ToList();
        return stores;
    }

    public async Task<StoreModel?> GetStoreByIdAsync(int id)
    {
        return null;
    }

    public async Task<(bool Success, string Message)> CreateStoreAsync(StoreModel store)
    {
        try
        {
            var response = await _api.PostAsync<object, JsonElement>("owner/shops", new
            {
                name = store.Name,
                description = store.Description,
                address = store.Address,
                imageUrl = store.ImageUrl,
                menuImagesUrl = store.MenuImagesUrl,
                category = string.IsNullOrWhiteSpace(store.Category) || store.Category == "M?c d?nh" ? null : store.Category,
                latitude = store.Latitude,
                longitude = store.Longitude
            });
            if (response.ValueKind != JsonValueKind.Undefined)
            {
                return (true, "Đăng ký gian hàng thành công!");
            }
            _logger.LogWarning("CreateStoreAsync: API returned empty/default response");
            return (false, "L?i khi dang ký gian hàng. Vui lòng th? l?i.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateStoreAsync failed");
            return (false, $"L?Lỗi khi đăng ký gian hàng: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> UpdateStoreAsync(StoreModel store)
    {
        try
        {
            var response = await _api.PutAsync<object, JsonElement>($"owner/shops/{store.Id}", new
            {
                name = store.Name,
                description = store.Description,
                address = store.Address,
                imageUrl = store.ImageUrl,
                menuImagesUrl = store.MenuImagesUrl,
                category = string.IsNullOrWhiteSpace(store.Category) || store.Category == "M?c d?nh" ? null : store.Category,
                latitude = store.Latitude,
                longitude = store.Longitude
            });
            if (response.ValueKind != JsonValueKind.Undefined)
            {
                return (true, "C?p nh?t thành công!");
            }
            _logger.LogWarning("UpdateStoreAsync: API returned empty/default response");
            return (false, "L?i khi c?p nh?t gian hàng.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateStoreAsync failed");
            return (false, $"L?i khi c?p nh?t gian hàng: {ex.Message}");
        }
    }

    public async Task<JsonElement> GenerateTTSAsync(int storeId, string langCode = "vi")
    {
        return await _api.PostAsync<object, JsonElement>($"owner/shops/{storeId}/generate-tts", new { langCode }, TimeSpan.FromMinutes(2));
    }

    public async Task<JsonElement> GenerateTTSWithTextAsync(int storeId, string text, string langCode = "vi")
    {
        return await _api.PostAsync<object, JsonElement>($"owner/shops/{storeId}/generate-tts", new { text, langCode }, TimeSpan.FromMinutes(2));
    }

    public async Task<JsonElement> GenerateTTSAllLanguagesAsync(int storeId, string? text = null)
    {
        return await _api.PostAsync<object, JsonElement>($"owner/shops/{storeId}/generate-tts-all", new { text }, TimeSpan.FromMinutes(4));
    }

    public async Task<string?> TranslateTextAsync(string text, string targetLang)
    {
        var response = await _api.PostAsync<object, JsonElement>("owner/shops/translate", new { text, targetLang });
        if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("translatedText", out var translatedProp))
        {
            return translatedProp.GetString();
        }
        return null;
    }

    public async Task<bool> DeleteStoreAsync(int id)
    {
        return await _api.DeleteAsync($"owner/shops/{id}");
    }

    public async Task<bool> ApproveStoreAsync(int id)
    {
        return await _api.PatchAsync($"admin/shops/{id}/approve");
    }

    public async Task<bool> RejectStoreAsync(int id)
    {
        return await _api.PatchAsync($"admin/shops/{id}/reject");
    }

    public async Task<List<CustomerModel>> GetAllUsersAsync()
    {
        return await _api.GetAsync<List<CustomerModel>>("admin/users") ?? new();
    }

    public async Task<List<SellerModel>> GetSellersAsync()
    {
        var sellers = await _api.GetAsync<List<SellerModel>>("admin/users/sellers") ?? new();
        var stores = await GetStoresAsync();
        foreach (var seller in sellers)
        {
            seller.Stores = stores.Where(s => s.SellerId == seller.Id).ToList();
        }
        return sellers;
    }

    public async Task<bool> UpdateSellerStatusAsync(int id, string status)
    {
        return await _api.PatchAsync($"admin/users/{id}/status?status={status}");
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _api.DeleteAsync($"admin/users/{id}");
    }

    public async Task<List<CustomerModel>> GetCustomersAsync()
    {
        return await _api.GetAsync<List<CustomerModel>>("admin/users/customers") ?? new();
    }

    public async Task<bool> SeedCustomersAsync()
    {
        var res = await _api.PostAsync<object, string>("admin/users/seed-customers", new { });
        return res != null;
    }

    public async Task<bool> UpdateCustomerStatusAsync(int id, string status)
    {
        return await _api.PatchAsync($"admin/users/{id}/status?status={status}");
    }

    public async Task<List<StatModel>> GetSellerStatsAsync(int sellerId, int? storeId = null)
    {
        var query = $"stats/seller/{sellerId}";
        if (storeId.HasValue && storeId.Value > 0)
        {
            query += $"?storeId={storeId.Value}";
        }
        return await _api.GetAsync<List<StatModel>>(query) ?? new List<StatModel>();
    }

    public async Task<List<ReviewModel>> GetStoreReviewsAsync(int storeId, int sellerId = 0)
    {
        var url = $"stores/{storeId}/reviews";
        if (storeId == 0 && sellerId > 0) url += $"?sellerId={sellerId}";
        return await _api.GetAsync<List<ReviewModel>>(url) ?? new List<ReviewModel>();
    }

    public async Task<AdminStatsModel?> GetAdminStatsAsync(int year, int? month = null)
    {
        var query = $"admin/stats?year={year}";
        if (month.HasValue)
            query += $"&month={month.Value}";
        return await _api.GetAsync<AdminStatsModel>(query);
    }

    public async Task<AdminRevenueModel?> GetAdminRevenueAsync(int? month, int year)
    {
        var query = $"admin/stats/revenue?year={year}";
        if (month.HasValue)
            query += $"&month={month.Value}";
        return await _api.GetAsync<AdminRevenueModel>(query);
    }

    public async Task<int> GetOnlineCountAsync()
    {
        var response = await _api.GetAsync<JsonElement>("admin/stats/online-count");
        if (response.ValueKind != JsonValueKind.Undefined &&
            response.TryGetProperty("count", out var countProp) &&
            countProp.TryGetInt32(out var count))
        {
            return count;
        }

        return 0;
    }

    public async Task<SellerRevenueModel?> GetSellerRevenueAsync(int sellerId, string? weekDate = null, int? storeId = null)
    {
        var query = $"stats/seller/{sellerId}/revenue";
        var param = new List<string>();
        if (!string.IsNullOrEmpty(weekDate)) param.Add($"week={weekDate}");
        if (storeId.HasValue && storeId.Value > 0) param.Add($"storeId={storeId.Value}");
        if (param.Any()) query += "?" + string.Join("&", param);

        return await _api.GetAsync<SellerRevenueModel>(query);
    }

    public async Task<SellerStatsOverviewModel?> GetSellerOverviewAsync(int sellerId, string period, string? anchorDate = null, int? storeId = null)
    {
        var query = $"stats/seller/{sellerId}/overview?period={period}";
        if (!string.IsNullOrWhiteSpace(anchorDate)) query += $"&anchorDate={anchorDate}";
        if (storeId.HasValue && storeId.Value > 0) query += $"&storeId={storeId.Value}";
        return await _api.GetAsync<SellerStatsOverviewModel>(query);
    }

    public async Task<bool> DeleteAdminStoreAudioAsync(int storeId, string languageCode)
    {
        return await _api.DeleteAsync($"admin/shops/{storeId}/audio/{Uri.EscapeDataString(languageCode)}");
    }

    public string ToAbsoluteMediaUrl(string? audioUrl)
    {
        if (string.IsNullOrWhiteSpace(audioUrl))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(audioUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        var baseUri = new Uri(new Uri("http://localhost:5279/api/"), ".");
        if (Uri.TryCreate(baseUri, audioUrl.TrimStart('/'), out var combinedUri))
        {
            return combinedUri.ToString();
        }

        return audioUrl;
    }
}

