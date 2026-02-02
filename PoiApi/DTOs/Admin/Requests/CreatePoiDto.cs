using System.ComponentModel.DataAnnotations;

namespace PoiApi.DTOs.Admin.Requests;

public class CreatePoiDto
{
    [Required]
    public required string Location { get; set; }

    public string? ImageUrl { get; set; }

    [Required]
    [MinLength(1)]
    public required List<CreatePoiTranslationDto> Translations { get; set; } = new();
}

public class CreatePoiTranslationDto
{
    [Required]
    public required string LanguageCode { get; set; } = String.Empty;

    [Required]
    public required string Name { get; set; } = String.Empty;

    public string? Description { get; set; } = String.Empty;
}
