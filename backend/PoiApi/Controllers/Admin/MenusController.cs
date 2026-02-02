using Microsoft.AspNetCore.Mvc;
using PoiApi.Data;
using PoiApi.DTOs;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;
using PoiApi.DTOs.App;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/pois/{poiId}/menus")]
    public class MenusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenusController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenu(
            int poiId,
            [FromBody] CreateMenuDto dto)
        {
            var poi = await _context.POIs.FindAsync(poiId);
            if (poi == null)
                return NotFound("POI not found");

            var menu = new Menu
            {
                Name = dto.Name,
                PoiId = poiId
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return Ok(new AppMenuDto
            {
                Id = menu.Id,
                Name = menu.Name
            });
        }

        // GET cho APP
        [HttpGet]
        public IActionResult GetMenus(int poiId)
        {
            var menus = _context.Menus
                .Where(m => m.PoiId == poiId)
                .Select(m => new AppMenuDto
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .ToList();

            return Ok(menus);
        }
    }
}
