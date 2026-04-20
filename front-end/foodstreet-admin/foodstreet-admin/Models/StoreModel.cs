namespace foodstreet_admin.Models;

public class StoreModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? MenuImagesUrl { get; set; }
    public string AudioUrl { get; set; } = "";
    public Dictionary<string, string> AudioUrls { get; set; } = new();
    public List<StoreAudioTranslationModel> AudioTranslations { get; set; } = new();
    public string Category { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public int SellerId { get; set; }
    public string SellerName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TotalListens { get; set; }
    public int TotalViews { get; set; }
    public double Rating { get; set; }
    public string? QrCodeUrl { get; set; }
}

public class StoreAudioTranslationModel
{
    public string LanguageCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string AudioUrl { get; set; } = "";
}

public class SellerModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Status { get; set; } = "Active";
    public DateTime RegisteredAt { get; set; } = DateTime.Now;
    public int StoreCount { get; set; }
    public List<StoreModel> Stores { get; set; } = new();
}

public class CustomerModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "USER";
    public string Status { get; set; } = "Active";
    public DateTime RegisteredAt { get; set; } = DateTime.Now;
    public int TotalListens { get; set; }
}

public class StatModel
{
    public int TotalListens { get; set; }
    public int TotalViews { get; set; }
    public int TotalReviews { get; set; }
    public string Month { get; set; } = "";
}

public class ReviewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int StoreId { get; set; }
}

public class SellerRevenueModel
{
    public string WeekStart { get; set; } = "";
    public string WeekEnd { get; set; } = "";
    public decimal TotalRevenue { get; set; }
    public List<DailyRevenueItem> DailyRevenue { get; set; } = new();
    public List<ShopRevenueItem> ShopRevenue { get; set; } = new();
}

public class DailyRevenueItem
{
    public string Date { get; set; } = "";
    public string DayName { get; set; } = "";
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class SellerStatsOverviewModel
{
    public string Period { get; set; } = "";
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public decimal TotalRevenue { get; set; }
    public int TotalListens { get; set; }
    public List<SellerRevenueBreakdownItem> RevenueBreakdown { get; set; } = new();
    public List<SellerListenBreakdownItem> ListenBreakdown { get; set; } = new();
    public List<ShopRevenueItem> RevenueByShop { get; set; } = new();
    public List<ShopListenItem> ListensByShop { get; set; } = new();
    public List<SellerTransactionItem> RecentTransactions { get; set; } = new();
}

public class SellerTransactionItem
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string PackageName { get; set; } = "";
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "";
}

public class SellerRevenueBreakdownItem
{
    public string Label { get; set; } = "";
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class SellerListenBreakdownItem
{
    public string Label { get; set; } = "";
    public int Listens { get; set; }
}
