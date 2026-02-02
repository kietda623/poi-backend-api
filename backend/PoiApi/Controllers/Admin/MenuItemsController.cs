using Microsoft.AspNetCore.Mvc;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.App;

[ApiController]
[Route("api/admin/menus/{menuId}/items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _service;

    public MenuItemsController(IMenuItemService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int menuId)
        => Ok(await _service.GetByMenuAsync(menuId));

    [HttpPost]
    public async Task<IActionResult> Create(int menuId, CreateMenuItemDto dto)
    {
        var result = await _service.CreateAsync(menuId, dto);
        return result == null ? NotFound("Menu not found") : Ok(result);
    }
}
