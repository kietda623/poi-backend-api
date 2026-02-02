using PoiApi.Models;

public class POITranslation
{
    public int Id { get; set; }

    public int POIId { get; set; }
    public POI POI { get; set; } = null!;

    public string LanguageCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}
