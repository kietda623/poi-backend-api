namespace AppUser.Models
{
    /// <summary>POI Translation (multi-language content)</summary>
    public class POITranslationDto
    {
        public int Id { get; set; }
        public int POIId { get; set; }
        public string LanguageCode { get; set; } = "vi"; // "vi" | "en"
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>Main POI Data Transfer Object</summary>
    public class POIDto
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Microsoft.Maui.Devices.Sensors.Location? LocationObj =>
            (Latitude.HasValue && Longitude.HasValue) ? new Microsoft.Maui.Devices.Sensors.Location(Latitude.Value, Longitude.Value) : null;
        public List<POITranslationDto> Translations { get; set; } = new();
        public ShopDto? Shop { get; set; }
        public List<AudioGuideDto> AudioGuides { get; set; } = new();
        public List<AppLanguageDto>? AvailableLanguages { get; set; } = new();

        // Helper: get translation by language code
        public POITranslationDto? GetTranslation(string langCode = "vi")
            => Translations.FirstOrDefault(t => t.LanguageCode == langCode)
            ?? Translations.FirstOrDefault();

        public string DisplayName(string lang = "vi")
            => GetTranslation(lang)?.Name ?? $"POI #{Id}";

        public string DisplayDescription(string lang = "vi")
            => GetTranslation(lang)?.Description ?? string.Empty;
    }
}
