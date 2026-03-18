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

    // ── MOCK DATA (thay bằng API call khi có backend) ─────────────

    private static List<StoreModel> _mockStores = new()
    {
        new() { Id=1, Name="Quán Ốc Bà Ba", SellerId=1, SellerName="Nguyễn Văn A",
                Category="Ốc", Address="123 Lê Lợi, Q.1, TP.HCM", Phone="0901234567",
                Status="Active", AudioUrl="audio1.mp3", TotalListens=1250, TotalViews=3480, Rating=4.7 },
        new() { Id=2, Name="Cơm Tấm Sài Gòn", SellerId=1, SellerName="Nguyễn Văn A",
                Category="Cơm", Address="45 Nguyễn Trãi, Q.5", Phone="0912345678",
                Status="Pending", TotalListens=0, TotalViews=0 },
        new() { Id=3, Name="Hải Sản Tươi Sống", SellerId=2, SellerName="Trần Thị B",
                Category="Hải sản", Address="78 Võ Văn Tần, Q.3", Phone="0923456789",
                Status="Pending", AudioUrl="audio3.mp3", TotalListens=0, TotalViews=0 },
        new() { Id=4, Name="Bún Bò Huế Cô Ba", SellerId=3, SellerName="Lê Văn C",
                Category="Bún/Phở", Address="99 CMT8, Q.10", Phone="0934567890",
                Status="Active", AudioUrl="audio4.mp3", TotalListens=980, TotalViews=2100, Rating=4.5 },
        new() { Id=5, Name="Trà Sữa Ding Tea", SellerId=3, SellerName="Lê Văn C",
                Category="Đồ uống", Address="55 Hoàng Văn Thụ, Q.Tân Bình", Phone="0945678901",
                Status="Rejected", TotalListens=0, TotalViews=0 },
    };

    private static List<SellerModel> _mockSellers = new()
    {
        new() { Id=1, FullName="Nguyễn Văn A", Email="seller1@mail.com", Phone="0901111111",
                Status="Active", RegisteredAt=new DateTime(2025,1,15), StoreCount=2 },
        new() { Id=2, FullName="Trần Thị B", Email="seller2@mail.com", Phone="0902222222",
                Status="Pending", RegisteredAt=new DateTime(2025,3,20), StoreCount=1 },
        new() { Id=3, FullName="Lê Văn C", Email="seller3@mail.com", Phone="0903333333",
                Status="Active", RegisteredAt=new DateTime(2025,2,10), StoreCount=3 },
        new() { Id=4, FullName="Phạm Thị D", Email="seller4@mail.com", Phone="0904444444",
                Status="Disabled", RegisteredAt=new DateTime(2024,12,5), StoreCount=1 },
        new() { Id=5, FullName="Võ Văn E", Email="seller5@mail.com", Phone="0905555555",
                Status="Pending", RegisteredAt=new DateTime(2025,5,1), StoreCount=0 },
    };

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
        // TODO: return await _api.GetAsync<List<StoreModel>>($"/stores{(sellerId.HasValue ? $"?sellerId={sellerId}" : "")}") ?? new();
        await Task.Delay(100);
        return sellerId.HasValue
            ? _mockStores.Where(s => s.SellerId == sellerId).ToList()
            : _mockStores.ToList();
    }

    public async Task<StoreModel?> GetStoreByIdAsync(int id)
    {
        await Task.Delay(50);
        return _mockStores.FirstOrDefault(s => s.Id == id);
    }

    public async Task<(bool Success, string Message)> CreateStoreAsync(StoreModel store)
    {
        await Task.Delay(300);
        // TODO: return await _api.PostAsync<StoreModel, (bool,string)>("/stores", store);
        store.Id = _mockStores.Count + 1;
        store.Status = "Pending";
        store.CreatedAt = DateTime.Now;
        _mockStores.Add(store);
        return (true, "Đăng ký gian hàng thành công! Đang chờ duyệt.");
    }

    public async Task<(bool Success, string Message)> UpdateStoreAsync(StoreModel store)
    {
        await Task.Delay(300);
        var idx = _mockStores.FindIndex(s => s.Id == store.Id);
        if (idx < 0) return (false, "Không tìm thấy gian hàng!");
        _mockStores[idx] = store;
        return (true, "Cập nhật thành công!");
    }

    public async Task<bool> DeleteStoreAsync(int id)
    {
        await Task.Delay(200);
        var store = _mockStores.FirstOrDefault(s => s.Id == id);
        if (store == null) return false;
        _mockStores.Remove(store);
        return true;
    }

    public async Task<bool> ApproveStoreAsync(int id)
    {
        await Task.Delay(200);
        // TODO: return await _api.PatchAsync($"/stores/{id}/approve");
        var store = _mockStores.FirstOrDefault(s => s.Id == id);
        if (store == null) return false;
        store.Status = "Active";
        return true;
    }

    public async Task<bool> RejectStoreAsync(int id)
    {
        await Task.Delay(200);
        var store = _mockStores.FirstOrDefault(s => s.Id == id);
        if (store == null) return false;
        store.Status = "Rejected";
        return true;
    }

    // ── Sellers ─────────────────────────────────────────────────────

    public async Task<List<SellerModel>> GetSellersAsync()
    {
        await Task.Delay(100);
        // Gán stores cho mỗi seller
        foreach (var seller in _mockSellers)
            seller.Stores = _mockStores.Where(s => s.SellerId == seller.Id).ToList();
        return _mockSellers.ToList();
    }

    public async Task<bool> UpdateSellerStatusAsync(int id, string status)
    {
        await Task.Delay(200);
        var seller = _mockSellers.FirstOrDefault(s => s.Id == id);
        if (seller == null) return false;
        seller.Status = status;
        return true;
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

    public async Task<List<StatModel>> GetSellerStatsAsync(int sellerId)
    {
        await Task.Delay(100);
        return new List<StatModel>
        {
            new() { Month="Tháng 1/2025", TotalListens=120, TotalViews=340, TotalReviews=5 },
            new() { Month="Tháng 2/2025", TotalListens=180, TotalViews=420, TotalReviews=8 },
            new() { Month="Tháng 3/2025", TotalListens=210, TotalViews=510, TotalReviews=11 },
            new() { Month="Tháng 4/2025", TotalListens=290, TotalViews=620, TotalReviews=9 },
            new() { Month="Tháng 5/2025", TotalListens=450, TotalViews=780, TotalReviews=14 },
        };
    }

    public async Task<List<ReviewModel>> GetStoreReviewsAsync(int storeId)
    {
        await Task.Delay(100);
        return new List<ReviewModel>
        {
            new() { CustomerName="Nguyễn Thị B", Rating=5, Comment="Quán ngon, phục vụ tốt!", CreatedAt=new DateTime(2025,5,12) },
            new() { CustomerName="Trần Văn C", Rating=4, Comment="Ốc tươi, giá hợp lý.", CreatedAt=new DateTime(2025,5,10) },
            new() { CustomerName="Lê Thị D", Rating=5, Comment="Audio giới thiệu rất hay!", CreatedAt=new DateTime(2025,5,8) },
            new() { CustomerName="Phạm Văn E", Rating=3, Comment="Không gian hơi chật.", CreatedAt=new DateTime(2025,5,5) },
        };
    }
}
