namespace foodstreet_admin.Models;

public class TourModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string EstimatedTime { get; set; } = "1 giờ";
    public List<int> StoreIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public string ImageUrl { get; set; } = "";
}
