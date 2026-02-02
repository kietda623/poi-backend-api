using Microsoft.AspNetCore.Mvc;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.App;

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
}
