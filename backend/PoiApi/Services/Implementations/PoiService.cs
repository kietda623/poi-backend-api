using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;
using PoiApi.Models;

public class PoiService : IPoiService
{
	private readonly AppDbContext _context;

	public PoiService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<List<POIAdminDto>> GetAllAsync(string lang)
	{
		var pois = await _context.POIs
			.Include(p => p.Translations)
			.ToListAsync();

		return pois.Select(p =>
		{
			var t = p.Translations.FirstOrDefault(x => x.LanguageCode == lang)
					?? p.Translations.First();

			return new POIAdminDto
			{
				Id = p.Id,
				ImageUrl = p.ImageUrl,
				Location = p.Location,
				Name = t.Name,
				Description = t.Description
			};
		}).ToList();
	}

	public async Task<POIAdminDto?> GetByIdAsync(int id, string lang)
	{
		var poi = await _context.POIs
			.Include(p => p.Translations)
			.FirstOrDefaultAsync(p => p.Id == id);

		if (poi == null) return null;

		var t = poi.Translations.First(x => x.LanguageCode == lang);

		return new POIAdminDto
		{
			Id = poi.Id,
			ImageUrl = poi.ImageUrl,
			Location = poi.Location,
			Name = t.Name,
			Description = t.Description
		};
	}

	public async Task<POIAdminDto> CreateAsync(CreatePoiDto dto)
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

		var first = poi.Translations.First();

		return new POIAdminDto
		{
			Id = poi.Id,
			ImageUrl = poi.ImageUrl,
			Location = poi.Location,
			Name = first.Name,
			Description = first.Description
		};
	}
}
