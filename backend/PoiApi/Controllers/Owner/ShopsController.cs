using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Owner;
using PoiApi.Models;
using PoiApi.Services;
using System.Security.Claims;

namespace PoiApi.Controllers.Owner
{
    [ApiController]
    [Route("api/owner/shops")]
    [Authorize(Roles = RoleConstants.Owner)]
    public class ShopsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AzureSpeechService _tts;
        private readonly AzureTranslationService _translator;

        public ShopsController(AppDbContext context, AzureSpeechService tts, AzureTranslationService translator)
        {
            _context = context;
            _tts = tts;
            _translator = translator;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdString, out var userId);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyShops()
        {
            var lang = Request.Headers["Accept-Language"].ToString().ToLower().Split(',')[0];
            if (string.IsNullOrEmpty(lang)) lang = "vi";

            var ownerId = GetCurrentUserId();
            var query = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .Where(s => s.OwnerId == ownerId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var shops = new List<object>();
            foreach (var s in query)
            {
                string name = s.Name;
                string description = s.Description ?? "";

                // Nếu yêu cầu không phải Tiếng Việt, thử tìm bản dịch hoặc dịch tự động
                if (lang != "vi")
                {
                    var trans = s.Poi?.Translations.FirstOrDefault(t => t.LanguageCode == lang);
                    if (trans != null)
                    {
                        name = trans.Name;
                        description = trans.Description;
                    }
                    else
                    {
                        // Dịch tự động nếu chưa có bản dịch trong DB
                        name = await _translator.TranslateAsync(s.Name, lang) ?? s.Name;
                        description = await _translator.TranslateAsync(s.Description ?? "", lang) ?? (s.Description ?? "");
                    }
                }

                shops.Add(new
                {
                    s.Id,
                    Name = name,
                    Description = description,
                    Address = s.Address ?? "",
                    Phone = "", 
                    ImageUrl = s.Poi != null ? s.Poi.ImageUrl : "",
                    MenuImagesUrl = s.Poi != null ? s.Poi.MenuImagesUrl : "",
                    AudioUrl = (s.Poi != null && s.Poi.Translations.Any(t => t.LanguageCode == lang)) 
                                ? s.Poi.Translations.First(t => t.LanguageCode == lang).AudioUrl : 
                                (s.Poi != null && s.Poi.Translations.Any(t => t.LanguageCode == "vi") ? s.Poi.Translations.First(t => t.LanguageCode == "vi").AudioUrl : ""),
                    Category = "Mặc định",
                    Status = s.IsActive ? "Active" : "Pending",
                    SellerId = s.OwnerId,
                    s.CreatedAt,
                    Latitude = s.Poi != null ? s.Poi.Latitude : 0.0,
                    Longitude = s.Poi != null ? s.Poi.Longitude : 0.0
                });
            }

            return Ok(shops);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShop([FromBody] ShopCreateDto dto)
        {
            var ownerId = GetCurrentUserId();

            // 1. Tạo POI tương ứng cho Shop
            var poi = new POI
            {
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                ImageUrl = dto.ImageUrl,
                MenuImagesUrl = dto.MenuImagesUrl,
                Location = dto.Address
            };

            var translation = new POITranslation
            {
                LanguageCode = "vi",
                Name = dto.Name,
                Description = dto.Description
            };
            poi.Translations.Add(translation);

            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();

            // 2. Tạo Shop liên kết với POI
            var shop = new Shop
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                OwnerId = ownerId,
                PoiId = poi.Id,
                IsActive = true // Mặc định cho phép hoạt động (Hoặc để Admin duyệt)
            };

            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();

            return Ok(new { id = shop.Id, message = "Đăng ký gian hàng thành công" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(int id, [FromBody] ShopCreateDto dto)
        {
            var ownerId = GetCurrentUserId();
            var shop = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

            if (shop == null) return NotFound("Gian hàng không tồn tại hoặc không thuộc quyền sở hữu");

            shop.Name = dto.Name;
            shop.Description = dto.Description;
            shop.Address = dto.Address;

            if (shop.Poi != null)
            {
                shop.Poi.Latitude = dto.Latitude;
                shop.Poi.Longitude = dto.Longitude;
                shop.Poi.ImageUrl = dto.ImageUrl;
                shop.Poi.MenuImagesUrl = dto.MenuImagesUrl;
                shop.Poi.Location = dto.Address;

                var trans = shop.Poi.Translations.FirstOrDefault(t => t.LanguageCode == "vi");
                if (trans != null)
                {
                    trans.Name = dto.Name;
                    trans.Description = dto.Description;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật gian hàng thành công" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShop(int id)
        {
            var ownerId = GetCurrentUserId();
            var shop = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

            if (shop == null) return NotFound("Gian hàng không tồn tại hoặc không thuộc quyền sở hữu");

            // Remove related POI translations and POI if exists
            if (shop.Poi != null)
            {
                _context.POITranslations.RemoveRange(shop.Poi.Translations);
                _context.POIs.Remove(shop.Poi);
            }

            _context.Shops.Remove(shop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    [HttpPost("{id}/generate-tts")]
    public async Task<IActionResult> GenerateTTS(int id, [FromBody] TTSRequestDto? dto = null)
    {
        var ownerId = GetCurrentUserId();
        var shop = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

        if (shop == null || shop.Poi == null) return NotFound("Gian hàng không tồn tại");

        var langCode = string.IsNullOrWhiteSpace(dto?.LangCode) ? "vi" : dto.LangCode.ToLower();

        // Tìm bản dịch tương ứng hoặc tạo mới nếu chưa có
        var translation = shop.Poi.Translations.FirstOrDefault(t => t.LanguageCode == langCode);
        
        bool isNew = false;
        if (translation == null)
        {
            isNew = true;
            translation = new POITranslation
            {
                POIId = shop.Poi.Id,
                LanguageCode = langCode,
                Name = shop.Name, 
                Description = shop.Description ?? ""
            };
        }

        // TỰ ĐỘNG DỊCH nếu ngôn ngữ không phải Tiếng Việt và không có Text thủ công
        if (langCode != "vi" && string.IsNullOrWhiteSpace(dto?.Text))
        {
            // Dịch Tên (Tùy chọn, ở đây dịch luôn cho đồng bộ)
            translation.Name = await _translator.TranslateAsync(shop.Name, langCode) ?? shop.Name;
            // Dịch Mô tả
            translation.Description = await _translator.TranslateAsync(shop.Description ?? "", langCode) ?? (shop.Description ?? "");
        }
        else if (!string.IsNullOrWhiteSpace(dto?.Text))
        {
            // Nếu có Text thủ công từ Seller, cập nhật vào mô tả bản dịch luôn
            translation.Description = dto.Text;
        }

        if (isNew) _context.POITranslations.Add(translation);
        else _context.POITranslations.Update(translation);

        await _context.SaveChangesAsync();

        // Chuẩn bị văn bản để nói (đã được dịch hoặc do user nhập)
        var textToSpeak = $"{translation.Name}. {translation.Description}";
        
        var audioUrl = await _tts.GenerateAudioAsync(shop.Poi.Id, langCode, textToSpeak);

        if (audioUrl != null)
        {
            translation.AudioUrl = audioUrl;
            await _context.SaveChangesAsync();
            return Ok(new { 
                audioUrl, 
                name = translation.Name, 
                description = translation.Description 
            });
        }

        return StatusCode(500, "Lỗi trong quá trình tạo audio từ Azure Speech Service");
    }
    }
}
