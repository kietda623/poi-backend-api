namespace PoiApi.DTOs.Admin.Responses;

public class POIAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string ImageUrl { get; set; } = String.Empty;
    public string Location { get; set; } = String.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? AudioUrl { get; set; }
}
