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
            // Nếu có sellerId, ưu tiên dùng API của Owner (Nếu đang ở vai trò Seller)
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
                latitude = store.Latitude,
                longitude = store.Longitude
            });
            if (response.ValueKind != JsonValueKind.Undefined)
            {
                return (true, "Đăng ký gian hàng thành công!");
            }
            _logger.LogWarning("CreateStoreAsync: API returned empty/default response");
            return (false, "Lỗi khi đăng ký gian hàng. Vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateStoreAsync failed");
            return (false, $"Lỗi khi đăng ký gian hàng: {ex.Message}");
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
                latitude = store.Latitude,
                longitude = store.Longitude
            });
            if (response.ValueKind != JsonValueKind.Undefined)
            {
                return (true, "Cập nhật thành công!");
            }
            _logger.LogWarning("UpdateStoreAsync: API returned empty/default response");
            return (false, "Lỗi khi cập nhật gian hàng.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateStoreAsync failed");
            return (false, $"Lỗi khi cập nhật gian hàng: {ex.Message}");
        }
    }

    public async Task<JsonElement> GenerateTTSAsync(int storeId, string langCode = "vi")
    {
        return await _api.PostAsync<object, JsonElement>($"owner/shops/{storeId}/generate-tts", new { langCode });
    }

    public async Task<JsonElement> GenerateTTSWithTextAsync(int storeId, string text, string langCode = "vi")
    {
        return await _api.PostAsync<object, JsonElement>($"owner/shops/{storeId}/generate-tts", new { text, langCode });
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

    // ── Sellers ─────────────────────────────────────────────────────

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

    // ── Customers ───────────────────────────────────────────────────

    public async Task<List<CustomerModel>> GetCustomersAsync()
    {
        return await _api.GetAsync<List<CustomerModel>>("admin/users/customers") ?? new();
    }

    public async Task<bool> UpdateCustomerStatusAsync(int id, string status)
    {
        return await _api.PatchAsync($"admin/users/{id}/status?status={status}");
    }

    // ── Stats ────────────────────────────────────────────────────────

    // ── Stats ────────────────────────────────────────────────────────

    public async Task<List<StatModel>> GetSellerStatsAsync(int sellerId)
    {
        return await _api.GetAsync<List<StatModel>>($"/stats/seller/{sellerId}") ?? new List<StatModel>();
    }

    public async Task<List<ReviewModel>> GetStoreReviewsAsync(int storeId)
    {
        return await _api.GetAsync<List<ReviewModel>>($"/stores/{storeId}/reviews") ?? new List<ReviewModel>();
    }
}