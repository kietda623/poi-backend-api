using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PoiApi.Data;
using PoiApi.DTOs.App;
using Microsoft.EntityFrameworkCore;
using PoiApi.Services;
using PoiApi.Models;
using System.Collections.Generic;

[ApiController]
[Route("api/app/pois")]
public class AppPoisController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly AzureTranslationService _translator;

    public AppPoisController(AppDbContext context, IMapper mapper, AzureTranslationService translator)
    {
        _context = context;
        _mapper = mapper;
        _translator = translator;
    }

    // 🔹 LIST (Home screen)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string lang = "vi")
    {
        // 1. Chỉ lấy những gian hàng đang hoạt động
        var shops = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .Where(s => s.IsActive && s.Poi != null)
            .ToListAsync();

        var resultList = new List<AppPoiListDto>();
        var namesToTranslate = new List<string>();
        var shopsNeedTranslation = new List<Tuple<int, string>>();

        foreach (var s in shops)
        {
            var p = s.Poi!;
            var t = p.Translations.FirstOrDefault(x => x.LanguageCode == lang);
            
            if (t != null)
            {
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? "",
                    Location = p.Location ?? "",
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = t.AudioUrl ?? p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? "",
                    Name = t.Name
                });
            }
            else if (lang == "vi")
            {
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? "",
                    Location = p.Location ?? "",
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? "",
                    Name = s.Name
                });
            }
            else
            {
                // Cần dịch tự động
                shopsNeedTranslation.Add(new Tuple<int, string>(p.Id, s.Name));
                namesToTranslate.Add(s.Name);
                
                resultList.Add(new AppPoiListDto
                {
                    Id = p.Id,
                    ImageUrl = p.ImageUrl ?? "",
                    Location = p.Location ?? "",
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    AudioUrl = p.Translations.FirstOrDefault(x => x.LanguageCode == "vi")?.AudioUrl ?? "",
                    Name = s.Name // Tạm thời để tên gốc, sẽ update sau batch translate
                });
            }
        }

        // BATCH TRANSLATE if needed
        if (lang != "vi" && namesToTranslate.Any())
        {
            var translatedNames = await _translator.TranslateListAsync(namesToTranslate, lang);
            if (translatedNames != null && translatedNames.Count == namesToTranslate.Count)
            {
                for (int i = 0; i < shopsNeedTranslation.Count; i++)
                {
                    var id = shopsNeedTranslation[i].Item1;
                    var item = resultList.FirstOrDefault(x => x.Id == id);
                    if (item != null) item.Name = translatedNames[i];
                }
            }
        }

        return Ok(resultList);
    }

    // 🔹 DETAIL (POI detail screen)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(
        int id,
        [FromQuery] string lang = "vi")
    {
        var shop = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .Include(s => s.Menus)
                .ThenInclude(m => m.MenuItems)
            .FirstOrDefaultAsync(s => s.PoiId == id && s.IsActive);

        if (shop == null || shop.Poi == null) return NotFound();

        var p = shop.Poi;
        var t = p.Translations.FirstOrDefault(x => x.LanguageCode == lang)
                ?? p.Translations.FirstOrDefault()
                ?? new POITranslation { Name = shop.Name, Description = shop.Description ?? "" };

        // AUTO-TRANSLATE POI Name and Description if not Vietnamese and translation missing/empty
        if (lang != "vi")
        {
            var existingT = p.Translations.FirstOrDefault(x => x.LanguageCode == lang);
            if (existingT == null)
            {
                t.Name = await _translator.TranslateAsync(shop.Name, lang) ?? shop.Name;
                t.Description = await _translator.TranslateAsync(shop.Description ?? "", lang) ?? (shop.Description ?? "");
            }
        }

        var dto = new AppPoiDetailDto
        {
            Id = p.Id,
            ImageUrl = p.ImageUrl ?? "",
            Location = p.Location ?? "",
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            AudioUrl = t.AudioUrl ?? "",
            Name = t.Name,
            Description = t.Description ?? "",
            Menus = shop.Menus?.Select(m => new AppMenuDto
            {
                Id = m.Id,
                Name = m.Name,
                Items = m.MenuItems.Select(i => new AppMenuItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price
                }).ToList()
            }).ToList() ?? new(),
            AvailableLanguages = p.Translations.Select(tr => new AppLanguageDto
            {
                Code = tr.LanguageCode,
                Name = GetLanguageName(tr.LanguageCode),
                HasAudio = !string.IsNullOrEmpty(tr.AudioUrl)
            }).ToList()
        };

        // AUTO-TRANSLATE Menu and Items if not Vietnamese
        if (lang != "vi" && dto.Menus.Any())
        {
            var textsToTranslate = new List<string>();
            foreach (var menu in dto.Menus)
            {
                textsToTranslate.Add(menu.Name);
                foreach (var item in menu.Items)
                {
                    textsToTranslate.Add(item.Name);
                }
            }

            var translatedTexts = await _translator.TranslateListAsync(textsToTranslate, lang);
            if (translatedTexts != null && translatedTexts.Count == textsToTranslate.Count)
            {
                int index = 0;
                foreach (var menu in dto.Menus)
                {
                    menu.Name = translatedTexts[index++];
                    foreach (var item in menu.Items)
                    {
                        item.Name = translatedTexts[index++];
                    }
                }
            }
        }

        return Ok(dto);
    }

    private string GetLanguageName(string code) => code.ToLower() switch
    {
        "vi" => "Tiếng Việt",
        "en" => "English",
        "zh" => "中文 (Chinese)",
        _ => code.ToUpper()
    };
}
