using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.App;

namespace PoiApi.Controllers.App
{
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

        // GET: api/app/pois?lang=vi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppPoiListDto>>> GetPois(
            [FromQuery] string lang = "vi")
        {
            var pois = await _context.POIs
                .Include(p => p.Translations)
                .ToListAsync();

            var result = pois.Select(p =>
            {
                var t = p.Translations
                    .FirstOrDefault(x => x.LanguageCode == lang)
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

        // GET: api/app/pois/1?lang=vi
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppPoiDetailDto>> GetPoiDetail(
            int id,
            [FromQuery] string lang = "vi")
        {
            var poi = await _context.POIs
                .Include(p => p.Translations)
                .Include(p => p.Menus)
                    .ThenInclude(m => m.MenuItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poi == null)
                return NotFound("POI not found");

            var t = poi.Translations
                .FirstOrDefault(x => x.LanguageCode == lang)
                ?? poi.Translations.First();

            var dto = _mapper.Map<AppPoiDetailDto>(poi);
            dto.Name = t.Name;
            dto.Description = t.Description;

            return Ok(dto);
        }
    }
}
