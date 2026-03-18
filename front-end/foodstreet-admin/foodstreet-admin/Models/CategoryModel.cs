namespace foodstreet_admin.Models;

public class CategoryModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int ParentId { get; set; }
    public string? ParentName { get; set; }
    public List<CategoryModel> Children { get; set; } = new();
    public int StoreCount { get; set; }
    public bool IsActive { get; set; } = true;
}
