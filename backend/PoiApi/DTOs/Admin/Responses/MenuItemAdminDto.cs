namespace PoiApi.DTOs.Admin.Responses;

public class MenuItemAdminDto
{
    public int Id { get; set; }
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
