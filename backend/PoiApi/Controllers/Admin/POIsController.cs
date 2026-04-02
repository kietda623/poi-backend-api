using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.App;
using PoiApi.Services;

[ApiController]
[Route("api/admin/pois")]
public class POIsController : ControllerBase
{
    private readonly IPoiService _poiService;

    public POIsController(IPoiService poiService)
    {
        _poiService = poiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string lang = "vi")
        => Ok(await _poiService.GetAllAsync(lang));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] string lang = "vi")
    {
        var poi = await _poiService.GetByIdAsync(id, lang);
        return poi == null ? NotFound() : Ok(poi);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePoiDto dto)
        => Ok(await _poiService.CreateAsync(dto));

    [HttpPost("{id}/generate-audio")]
    public async Task<IActionResult> GenerateAudio(
    int id,
    [FromQuery] string lang,
    [FromServices] AzureSpeechService tts,
    [FromServices] AppDbContext context)
    {
        var translation = await context.POITranslations
            .FirstOrDefaultAsync(t => t.POIId == id && t.LanguageCode == lang);

        if (translation == null)
            return NotFound("Translation not found");

        // Combine name + description as the spoken text
        var text = $"{translation.Name}. {translation.Description}";

        var audioUrl = await tts.GenerateAudioAsync(id, lang, text);

        if (audioUrl == null)
            return StatusCode(500, "Failed to generate audio");

        // Save audio URL to database
        translation.AudioUrl = audioUrl;
        await context.SaveChangesAsync();

        return Ok(new { audioUrl });
    }

    [HttpGet("{id}/audio")]
    public async Task<IActionResult> GetAudio(
        int id,
        [FromQuery] string lang,
        [FromServices] AppDbContext context)
    {
        var translation = await context.POITranslations
            .FirstOrDefaultAsync(t => t.POIId == id && t.LanguageCode == lang);

        if (translation == null || translation.AudioUrl == null)
            return NotFound("Audio not found");

        return Ok(new { audioUrl = translation.AudioUrl });
    }
}
