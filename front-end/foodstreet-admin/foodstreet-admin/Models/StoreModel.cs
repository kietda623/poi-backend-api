namespace foodstreet_admin.Models;

public class StoreModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string AudioUrl { get; set; } = "";
    public string Category { get; set; } = "";
    public string Status { get; set; } = "Pending"; // Pending | Active | Rejected
    public int SellerId { get; set; }
    public string SellerName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int TotalListens { get; set; }
    public int TotalViews { get; set; }
    public double Rating { get; set; }
}

public class SellerModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Status { get; set; } = "Active"; // Active | Pending | Disabled
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
    public string Status { get; set; } = "Active"; // Active | Disabled
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
