using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Owner.Menu;
using PoiApi.Models;
using System.Security.Claims;

namespace PoiApi.Controllers.Owner
{
    [ApiController]
    [Route("api/owner/menus")]
    [Authorize(Roles = RoleConstants.Owner)]
    public class MenusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenusController(AppDbContext context)
        {
            _context = context;
        }

        private Shop? GetMyShop()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            return _context.Shops
                .Include(s => s.Menus)
                .FirstOrDefault(s => s.OwnerId == userId);
        }

        // GET api/owner/menus
        [HttpGet]
        public IActionResult GetMyMenus()
        {
            var shop = GetMyShop();
            if (shop == null)
                return BadRequest("Owner does not have a shop");

            var menus = shop.Menus
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Description,
                    m.IsActive,
                    m.DisplayOrder
                });

            return Ok(menus);
        }

        // POST api/owner/menus
        [HttpPost]
        public IActionResult CreateMenu(CreateMenuDto dto)
        {
            var shop = GetMyShop();
            if (shop == null)
                return BadRequest("Owner does not have a shop");

            var menu = new Menu
            {
                Name = dto.Name,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder,
                ShopId = shop.Id
            };

            _context.Menus.Add(menu);
            _context.SaveChanges();

            return Ok(menu);
        }

        // PUT api/owner/menus/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateMenu(int id, UpdateMenuDto dto)
        {
            var shop = GetMyShop();
            if (shop == null)
                return BadRequest("Owner does not have a shop");

            var menu = _context.Menus
                .FirstOrDefault(m => m.Id == id && m.ShopId == shop.Id);

            if (menu == null)
                return NotFound("Menu not found");

            menu.Name = dto.Name;
            menu.Description = dto.Description;
            menu.IsActive = dto.IsActive;
            menu.DisplayOrder = dto.DisplayOrder;

            _context.SaveChanges();

            return Ok(menu);
        }

        // DELETE api/owner/menus/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteMenu(int id)
        {
            var shop = GetMyShop();
            if (shop == null)
                return BadRequest("Owner does not have a shop");

            var menu = _context.Menus
                .FirstOrDefault(m => m.Id == id && m.ShopId == shop.Id);

            if (menu == null)
                return NotFound("Menu not found");

            _context.Menus.Remove(menu);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
