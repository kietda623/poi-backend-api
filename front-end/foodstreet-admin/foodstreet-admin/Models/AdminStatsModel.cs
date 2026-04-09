namespace foodstreet_admin.Models;

public class AdminStatsModel
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalListens { get; set; }
    public List<ShopRevenueItem> RevenueByShop { get; set; } = new();
    public List<MonthlyRevenueItem> RevenueByMonth { get; set; } = new();
    public List<ShopListenItem> ListensByShop { get; set; } = new();
    public List<MonthlyListenItem> ListensByMonth { get; set; } = new();
}

public class AdminRevenueModel
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<ShopRevenueItem> RevenueByShop { get; set; } = new();
    public List<MonthlyRevenueItem> RevenueByMonth { get; set; } = new();
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
