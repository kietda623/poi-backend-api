namespace PoiApi.DTOs.Admin.Requests;

public class CreateMenuItemDto
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}
