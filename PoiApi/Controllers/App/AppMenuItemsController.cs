using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;

namespace PoiApi.Controllers.App
{
    [ApiController]
    [Route("api/app/menus/{menuId}/items")]
    public class AppMenuItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppMenuItemsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems(int menuId)
        {
            var items = await _context.MenuItems
                .Where(i => i.MenuId == menuId)
                .Select(i => new AppMenuItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price
                })
                .ToListAsync();

            return Ok(items);
        }
    }

}
