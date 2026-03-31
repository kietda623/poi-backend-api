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

    private static List<CustomerModel> _mockCustomers = new()
    {
        new() { Id=1, FullName="Nguyễn Thị A", Email="cust1@mail.com", Phone="0911111111",
                Status="Active", RegisteredAt=new DateTime(2025,1,10), TotalListens=450 },
        new() { Id=2, FullName="Trần Văn B", Email="cust2@mail.com", Phone="0922222222",
                Status="Active", RegisteredAt=new DateTime(2025,2,15), TotalListens=230 },
        new() { Id=3, FullName="Lê Thị C", Email="cust3@mail.com", Phone="0933333333",
                Status="Disabled", RegisteredAt=new DateTime(2025,1,20), TotalListens=50 },
        new() { Id=4, FullName="Phạm Văn D", Email="cust4@mail.com", Phone="0944444444",
                Status="Active", RegisteredAt=new DateTime(2025,3,5), TotalListens=780 },
        new() { Id=5, FullName="Võ Thị E", Email="cust5@mail.com", Phone="0955555555",
                Status="Active", RegisteredAt=new DateTime(2025,4,12), TotalListens=120 },
    };

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
        await Task.Delay(100);
        return _mockCustomers.ToList();
    }

    public async Task<bool> UpdateCustomerStatusAsync(int id, string status)
    {
        await Task.Delay(200);
        var c = _mockCustomers.FirstOrDefault(c => c.Id == id);
        if (c == null) return false;
        c.Status = status;
        return true;
    }

    // ── Stats ────────────────────────────────────────────────────────

    // ── Stats ────────────────────────────────────────────────────────

    public async Task<List<StatModel>> GetSellerStatsAsync(int sellerId)
    {
        await Task.Delay(100);

        // TODO: Khi nào Backend có API, hãy mở comment dòng dưới và xóa dòng return new List
        return await _api.GetAsync<List<StatModel>>($"/stats/seller/{sellerId}") ?? new List<StatModel>();

        // Tạm thời trả về list rỗng để tài khoản mới không bị hiển thị rác
        return new List<StatModel>();
    }

    public async Task<List<ReviewModel>> GetStoreReviewsAsync(int storeId)
    {
        await Task.Delay(100);

        // TODO: Tương tự, gọi API thật khi đã sẵn sàng
        return await _api.GetAsync<List<ReviewModel>>($"/stores/{storeId}/reviews") ?? new List<ReviewModel>();

        // Tạm thời trả về rỗng
        return new List<ReviewModel>();
    }
}