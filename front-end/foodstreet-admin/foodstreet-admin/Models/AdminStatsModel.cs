namespace foodstreet_admin.Models;

public class AdminStatsModel
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public AdminStatsSummary Summary { get; set; } = new();
    public SellerStatsSection SellerStats { get; set; } = new();
    public UserStatsSection UserStats { get; set; } = new();
    public List<MonthlyRevenueBreakdown> RevenueByMonth { get; set; } = new();
    public int TotalListens { get; set; }
    public int GuestListens { get; set; }
    public int RegisteredListens { get; set; }
    public List<MonthlyListenItem> ListensByMonth { get; set; } = new();
    public List<ShopListenItem> ListensByShop { get; set; } = new();
    public List<ShopRevenueItem> RevenueByShop { get; set; } = new(); // Legacy
}

public class AdminRevenueModel : AdminStatsModel
{
}

public class AdminStatsSummary
{
    public decimal TotalRevenue { get; set; }
    public decimal SellerRevenue { get; set; }
    public decimal UserRevenueTotal { get; set; }
    public decimal PlatformShare { get; set; }
    public decimal UserSharePercent { get; set; }
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int NewSubscriptionsThisMonth { get; set; }
}

public class SellerStatsSection
{
    public decimal TotalRevenue { get; set; }
    public int SubscriptionCount { get; set; }
    public List<TierRevenueItem> ByTier { get; set; } = new();
    public List<SellerRevenueDetail> BySeller { get; set; } = new();
}

public class UserStatsSection
{
    public decimal TotalRevenue { get; set; }
    public decimal PlatformSharePercent { get; set; }
    public decimal PlatformShareAmount { get; set; }
    public int SubscriptionCount { get; set; }
    public List<TierRevenueItem> ByTier { get; set; } = new();
}

public class TierRevenueItem
{
    public string Tier { get; set; } = "";
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class SellerRevenueDetail
{
    public int SellerId { get; set; }
    public string SellerName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PackageName { get; set; } = "";
    public decimal Revenue { get; set; }
    public int SubscriptionCount { get; set; }
}

public class MonthlyRevenueBreakdown
{
    public int Month { get; set; }
    public decimal SellerRevenue { get; set; }
    public decimal UserRevenue { get; set; }
    public decimal UserRevenueTotal { get; set; }
    public decimal TotalRevenue { get; set; }
    public int SubscriptionCount { get; set; }
}

public class ShopRevenueItem
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = "";
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class MonthlyRevenueItem
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class ShopListenItem
{
    public int ShopId { get; set; }
    public string ShopName { get; set; } = "";
    public int Listens { get; set; }
}

public class MonthlyListenItem
{
    public int Month { get; set; }
    public int Listens { get; set; }
}
