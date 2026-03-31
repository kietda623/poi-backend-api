namespace foodstreet_admin.Models;

/// <summary>Gói dịch vụ mà Admin tạo ra (Basic / Premium / VIP)</summary>
public class ServicePackageModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    /// <summary>Basic | Premium | VIP</summary>
    public string Tier { get; set; } = "Basic";
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Description { get; set; } = "";
    /// <summary>Danh sách tính năng (mỗi dòng 1 tính năng)</summary>
    public List<string> Features { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public int MaxStores { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Đăng ký gói dịch vụ của một Seller</summary>
public class SubscriptionModel
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string SellerName { get; set; } = "";
    public string SellerEmail { get; set; } = "";
    public int PackageId { get; set; }
    public string PackageName { get; set; } = "";
    /// <summary>Basic | Premium | VIP</summary>
    public string PackageTier { get; set; } = "Basic";
    /// <summary>Monthly | Yearly</summary>
    public string BillingCycle { get; set; } = "Monthly";
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    /// <summary>Active | Expired | Cancelled | Pending</summary>
    public string Status { get; set; } = "Pending";
}
