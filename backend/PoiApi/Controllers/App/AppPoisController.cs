using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PoiApi.Data;
using PoiApi.DTOs.App;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/app/pois")]
public class AppPoisController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AppPoisController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // 🔹 LIST (Home screen)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string lang = "vi")
    {
        var pois = await _context.POIs
            .Include(p => p.Translations)
            .ToListAsync();

        var result = pois.Select(p =>
        {
            var t = p.Translations.FirstOrDefault(x => x.LanguageCode == lang)
                    ?? p.Translations.First();

            return new AppPoiListDto
            {
                Id = p.Id,
                ImageUrl = p.ImageUrl,
                Name = t.Name
            };
        });

        return Ok(result);
    }

    // 🔹 DETAIL (POI detail screen)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(
        int id,
        [FromQuery] string lang = "vi")
    {
        var poi = await _context.POIs
            .Include(p => p.Translations)
            .Include(p => p.Menus)
                .ThenInclude(m => m.MenuItems)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (poi == null) return NotFound();

        var t = poi.Translations.FirstOrDefault(x => x.LanguageCode == lang)
                ?? poi.Translations.First();

        var dto = new AppPoiDetailDto
        {
            Id = poi.Id,
            ImageUrl = poi.ImageUrl,
            Location = poi.Location,
            Name = t.Name,
            Description = t.Description,
            Menus = poi.Menus.Select(m => new AppMenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Items = m.MenuItems.Select(i => new AppMenuItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price
                }).ToList()
            }).ToList()
        };

        return Ok(dto);
    }
}
