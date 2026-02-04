using Microsoft.AspNetCore.Mvc;
using PoiApi.DTOs.Admin.Requests;

[ApiController]
[Route("api/admin/shops/{shopId}/menus")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenusController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    // GET: api/admin/shops/{shopId}/menus
    [HttpGet]
    public async Task<IActionResult> GetByShop(int shopId)
    {
        var menus = await _menuService.GetByShopAsync(shopId);
        return Ok(menus);
    }

    // POST: api/admin/shops/{shopId}/menus
    [HttpPost]
    public async Task<IActionResult> Create(int shopId, CreateMenuDto dto)
    {
        var menu = await _menuService.CreateAsync(shopId, dto);
        if (menu == null) return NotFound("Shop not found");

        return Ok(menu);
    }
}
