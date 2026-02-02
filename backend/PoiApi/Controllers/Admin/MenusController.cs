using Microsoft.AspNetCore.Mvc;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.App;

[ApiController]
[Route("api/admin/pois/{poiId}/menus")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenusController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int poiId)
        => Ok(await _menuService.GetByPoiAsync(poiId));

    [HttpPost]
    public async Task<IActionResult> Create(int poiId, CreateMenuDto dto)
    {
        var result = await _menuService.CreateAsync(poiId, dto);
        return result == null ? NotFound("POI not found") : Ok(result);
    }
}
