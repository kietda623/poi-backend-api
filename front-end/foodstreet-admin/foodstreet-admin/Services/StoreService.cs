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



    // ── Stores ──────────────────────────────────────────────────────

    public async Task<List<StoreModel>> GetStoresAsync(int? sellerId = null)
    {
        var stores = await _api.GetAsync<List<StoreModel>>("admin/shops") ?? new();
        if (sellerId.HasValue)
            return stores.Where(s => s.SellerId == sellerId.Value).ToList();
        return stores;
    }

    public async Task<StoreModel?> GetStoreByIdAsync(int id)
    {
        return null; // Tương lai có API GetStoreById thì update
    }

    public async Task<(bool Success, string Message)> CreateStoreAsync(StoreModel store)
    {
        return (false, "Admin không hỗ trợ tạo gian hàng");
    }

    public async Task<(bool Success, string Message)> UpdateStoreAsync(StoreModel store)
    {
        return (false, "Chưa hỗ trợ");
    }

    public async Task<bool> DeleteStoreAsync(int id)
    {
        return await _api.DeleteAsync($"admin/shops/{id}");
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