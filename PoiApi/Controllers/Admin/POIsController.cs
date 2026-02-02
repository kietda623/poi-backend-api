using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/pois")]
    public class POIsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public POIsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 🔹 GET: api/admin/pois
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POIAdminDto>>> GetAll()
        {
            var pois = await _context.POIs
                .Include(p => p.Translations)
                .ToListAsync();

            return Ok(_mapper.Map<List<POIAdminDto>>(pois));
        }

        // 🔹 GET: api/admin/pois/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<POIAdminDto>> GetById(int id)
        {
            var poi = await _context.POIs
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poi == null)
                return NotFound("POI not found");

            return Ok(_mapper.Map<POIAdminDto>(poi));
        }

        // 🔹 POST: api/admin/pois
        [HttpPost]
        public async Task<ActionResult<POIAdminDto>> Create(
            [FromBody] CreatePoiDto dto)
        {
            var poi = new POI
            {
                ImageUrl = dto.ImageUrl,
                Location = dto.Location,
                Translations = dto.Translations.Select(t => new POITranslation
                {
                    LanguageCode = t.LanguageCode,
                    Name = t.Name,
                    Description = t.Description
                }).ToList()
            };

            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = poi.Id },
                _mapper.Map<POIAdminDto>(poi)
            );
        }

        // 🔹 POST: api/admin/pois/{id}/translations
        [HttpPost("{id:int}/translations")]
        public async Task<IActionResult> AddTranslation(
            int id,
            [FromBody] CreatePoiTranslationDto dto)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null)
                return NotFound("POI not found");

            var translation = new POITranslation
            {
                POIId = id,
                LanguageCode = dto.LanguageCode,
                Name = dto.Name,
                Description = dto.Description
            };

            _context.POITranslations.Add(translation);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
