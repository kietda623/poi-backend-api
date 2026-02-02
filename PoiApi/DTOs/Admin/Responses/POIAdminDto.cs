namespace PoiApi.DTOs.Admin.Responses;

public class POIAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string ImageUrl { get; set; } = String.Empty;
    public string Location { get; set; } = String.Empty;
}
