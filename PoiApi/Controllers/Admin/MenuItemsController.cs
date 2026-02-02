using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs;
using PoiApi.DTOs.Admin;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.App;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/menus/{menuId}/items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenuItemsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/menus/1/items create menu item
        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(
            int menuId,
            [FromBody] CreateMenuItemDto dto)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu == null) return NotFound("Menu not found");

            var item = new MenuItem
            {
                MenuId = menuId,
                Name = dto.Name,
                Price = dto.Price
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            return Ok(new AppMenuItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price
            }
                );
        }

        // GET: api/menus/1/items
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
