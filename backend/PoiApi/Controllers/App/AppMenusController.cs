using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;

namespace PoiApi.Controllers.App
{
    [ApiController]
    [Route("api/app/pois/{poiId}/menus")]
    public class AppMenusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppMenusController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenus(int poiId)
        {
            var menus = await _context.Menus
                .Where(m => m.PoiId == poiId)
                .Select(m => new AppMenuDto
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .ToListAsync();

            return Ok(menus);
        }
    }

}
