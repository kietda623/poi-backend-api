namespace PoiApi.DTOs.App
{
    public class AppPoiDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string Location { get; set; } = null!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AudioUrl { get; set; }
        public List<AppMenuDto> Menus { get; set; } = new();
        public List<AppLanguageDto> AvailableLanguages { get; set; } = new();
    }
}
