using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Owner;
using PoiApi.Models;
using PoiApi.Services;
using System.Globalization;
using System.Text.RegularExpressions;
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
        private readonly SubscriptionAccessService _subscriptionAccessService;

        public ShopsController(AppDbContext context, AzureSpeechService tts, AzureTranslationService translator, SubscriptionAccessService subscriptionAccessService)
        {
            _context = context;
            _tts = tts;
            _translator = translator;
            _subscriptionAccessService = subscriptionAccessService;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdString, out var userId);
            return userId;
        }

        private async Task<Subscription?> GetActiveSellerSubscriptionAsync(int ownerId)
        {
            return await _subscriptionAccessService.GetActiveSubscriptionAsync(ownerId, RoleConstants.Owner);
        }

        private async Task<int?> ResolveOrCreateCategoryIdAsync(string? categoryValue)
        {
            if (string.IsNullOrWhiteSpace(categoryValue))
                return null;

            var input = categoryValue.Trim();
            var lowerInput = input.ToLowerInvariant();

            var existing = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Name.ToLower() == lowerInput || c.Slug.ToLower() == lowerInput);

            if (existing != null)
                return existing.Id;

            // Auto-create category when seller uses a category name from UI.
            var baseSlug = ToSlug(input);
            var slug = baseSlug;
            var i = 1;
            while (await _context.Categories.AnyAsync(c => c.Slug == slug))
            {
                slug = $"{baseSlug}-{i++}";
            }

            var category = new Category
            {
                Name = input,
                Slug = slug,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category.Id;
        }

        private static string ToSlug(string value)
        {
            // Normalize vietnamese diacritics -> base chars.
            var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(ch);
            }

            var ascii = stringBuilder.ToString().ToLowerInvariant();
            // Replace spaces and separators with '-'
            ascii = Regex.Replace(ascii, @"[\s_/]+", "-");
            // Remove remaining non alphanumeric/hyphen
            ascii = Regex.Replace(ascii, @"[^a-z0-9\-]+", "");
            // Collapse multiple '-'
            ascii = Regex.Replace(ascii, @"-+", "-").Trim('-');
            return string.IsNullOrWhiteSpace(ascii) ? "category" : ascii;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyShops()
        {
            var lang = Request.Headers["Accept-Language"].ToString().ToLower().Split(',')[0];
            if (string.IsNullOrEmpty(lang)) lang = "vi";

            var ownerId = GetCurrentUserId();
            var query = await _context.Shops
                .Include(s => s.Category)
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

                // Náº¿u yÃªu cáº§u khÃ´ng pháº£i Tiáº¿ng Viá»‡t, thá»­ tÃ¬m báº£n dá»‹ch hoáº·c dá»‹ch tá»± Ä‘á»™ng
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
                        // Dá»‹ch tá»± Ä‘á»™ng náº¿u chÆ°a cÃ³ báº£n dá»‹ch trong DB
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
                    AudioUrls = s.Poi != null 
                                ? s.Poi.Translations.Where(t => !string.IsNullOrEmpty(t.AudioUrl)).ToDictionary(t => t.LanguageCode, t => t.AudioUrl) 
                                : new Dictionary<string, string>(),
                    Category = s.Category != null ? s.Category.Name : "Máº·c Ä‘á»‹nh",
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
            var activeSubscription = await GetActiveSellerSubscriptionAsync(ownerId);
            if (activeSubscription?.ServicePackage == null)
            {
                return BadRequest(new { message = "Ban can dang ky goi seller truoc khi tao gian hang." });
            }

            var currentStores = await _context.Shops.CountAsync(s => s.OwnerId == ownerId);
            if (currentStores >= activeSubscription.ServicePackage.MaxStores)
            {
                return BadRequest(new { message = $"Goi hien tai chi cho phep toi da {activeSubscription.ServicePackage.MaxStores} gian hang." });
            }

            var categoryId = await ResolveOrCreateCategoryIdAsync(dto.Category);

            // 1. Táº¡o POI tÆ°Æ¡ng á»©ng cho Shop
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

            // 2. Táº¡o Shop liÃªn káº¿t vá»›i POI
            var shop = new Shop
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                OwnerId = ownerId,
                PoiId = poi.Id,
                CategoryId = categoryId,
                IsActive = true // Máº·c Ä‘á»‹nh cho phÃ©p hoáº¡t Ä‘á»™ng (Hoáº·c Ä‘á»ƒ Admin duyá»‡t)
            };

            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();

            return Ok(new { id = shop.Id, message = "ÄÄƒng kÃ½ gian hÃ ng thÃ nh cÃ´ng" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(int id, [FromBody] ShopCreateDto dto)
        {
            var ownerId = GetCurrentUserId();
            var shop = await _context.Shops
                .Include(s => s.Category)
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

            if (shop == null) return NotFound("Gian hÃ ng khÃ´ng tá»“n táº¡i hoáº·c khÃ´ng thuá»™c quyá»n sá»Ÿ há»¯u");

            shop.Name = dto.Name;
            shop.Description = dto.Description;
            shop.Address = dto.Address;
            shop.CategoryId = await ResolveOrCreateCategoryIdAsync(dto.Category);

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
            return Ok(new { message = "Cáº­p nháº­t gian hÃ ng thÃ nh cÃ´ng" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShop(int id)
        {
            var ownerId = GetCurrentUserId();
            var shop = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

            if (shop == null) return NotFound("Gian hÃ ng khÃ´ng tá»“n táº¡i hoáº·c khÃ´ng thuá»™c quyá»n sá»Ÿ há»¯u");

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
        var activeSubscription = await GetActiveSellerSubscriptionAsync(ownerId);
        if (activeSubscription == null)
            return BadRequest(new { message = "Ban can co goi seller dang hoat dong de tao audio thuyet minh." });
        var shop = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

        if (shop == null || shop.Poi == null) return NotFound("Gian hÃ ng khÃ´ng tá»“n táº¡i");

        var langCode = string.IsNullOrWhiteSpace(dto?.LangCode) ? "vi" : dto.LangCode.ToLower();

        // TÃ¬m báº£n dá»‹ch tÆ°Æ¡ng á»©ng hoáº·c táº¡o má»›i náº¿u chÆ°a cÃ³
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

        // Tá»° Äá»˜NG Dá»ŠCH náº¿u ngÃ´n ngá»¯ khÃ´ng pháº£i Tiáº¿ng Viá»‡t vÃ  khÃ´ng cÃ³ Text thá»§ cÃ´ng
        if (langCode != "vi" && string.IsNullOrWhiteSpace(dto?.Text))
        {
            // Dá»‹ch TÃªn (TÃ¹y chá»n, á»Ÿ Ä‘Ã¢y dá»‹ch luÃ´n cho Ä‘á»“ng bá»™)
            translation.Name = await _translator.TranslateAsync(shop.Name, langCode) ?? shop.Name;
            // Dá»‹ch MÃ´ táº£
            translation.Description = await _translator.TranslateAsync(shop.Description ?? "", langCode) ?? (shop.Description ?? "");
        }
        else if (!string.IsNullOrWhiteSpace(dto?.Text))
        {
            // Náº¿u cÃ³ Text thá»§ cÃ´ng tá»« Seller, cáº­p nháº­t vÃ o mÃ´ táº£ báº£n dá»‹ch luÃ´n
            translation.Description = dto.Text;
        }

        if (isNew) _context.POITranslations.Add(translation);
        else _context.POITranslations.Update(translation);

        await _context.SaveChangesAsync();

        // Chuáº©n bá»‹ vÄƒn báº£n Ä‘á»ƒ nÃ³i (Ä‘Ã£ Ä‘Æ°á»£c dá»‹ch hoáº·c do user nháº­p)
        var textToSpeak = $"{translation.Name}. {translation.Description}";
        
        var audioUrl = await _tts.GenerateAudioAsync(shop.Poi.Id, langCode, textToSpeak);

        if (!string.IsNullOrEmpty(audioUrl))
        {
            translation.AudioUrl = audioUrl;
            _context.POITranslations.Update(translation);
            await _context.SaveChangesAsync();
        }

        return Ok(new { audioUrl, name = translation.Name, description = translation.Description });
    }

    [HttpPost("translate")]
    public async Task<IActionResult> TranslateText([FromBody] TranslateRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text)) return BadRequest("Ná»™i dung khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng");
        
        var translated = await _translator.TranslateAsync(dto.Text, dto.TargetLang);
        return Ok(new { translatedText = translated ?? dto.Text });
    }

    /// <summary>
    /// Generate TTS audio for ALL 3 languages (vi, en, zh) in one shot.
    /// Automatically translates Vietnamese content to EN & ZH, then generates audio.
    /// </summary>
    [HttpPost("{id}/generate-tts-all")]
    public async Task<IActionResult> GenerateTTSAll(int id, [FromBody] TTSRequestDto? dto = null)
    {
        var ownerId = GetCurrentUserId();
        var activeSubscription = await GetActiveSellerSubscriptionAsync(ownerId);
        if (activeSubscription == null)
            return BadRequest(new { message = "Ban can co goi seller dang hoat dong de tao audio thuyet minh." });
        var shop = await _context.Shops
            .Include(s => s.Poi)
                .ThenInclude(p => p.Translations)
            .FirstOrDefaultAsync(s => s.Id == id && s.OwnerId == ownerId);

        if (shop == null || shop.Poi == null) return NotFound("Gian hÃ ng khÃ´ng tá»“n táº¡i");

        var poi = shop.Poi;
        var languages = new[] { "vi", "en", "zh" };
        var results = new List<object>();

        // Step 1: Ensure Vietnamese translation exists with correct content
        var viTrans = poi.Translations.FirstOrDefault(t => t.LanguageCode == "vi");
        if (viTrans == null)
        {
            viTrans = new POITranslation
            {
                POIId = poi.Id,
                LanguageCode = "vi",
                Name = shop.Name,
                Description = dto?.Text ?? shop.Description ?? ""
            };
            _context.POITranslations.Add(viTrans);
        }
        else
        {
            viTrans.Name = shop.Name;
            if (!string.IsNullOrWhiteSpace(dto?.Text))
                viTrans.Description = dto.Text;
            else if (string.IsNullOrWhiteSpace(viTrans.Description))
                viTrans.Description = shop.Description ?? "";
        }
        await _context.SaveChangesAsync();

        // The base Vietnamese text to translate from
        var viName = viTrans.Name;
        var viDescription = viTrans.Description;

        // Step 2: Process each language
        foreach (var lang in languages)
        {
            var translation = poi.Translations.FirstOrDefault(t => t.LanguageCode == lang);
            bool isNew = false;

            if (translation == null)
            {
                isNew = true;
                translation = new POITranslation
                {
                    POIId = poi.Id,
                    LanguageCode = lang,
                    Name = viName,
                    Description = viDescription
                };
            }

            // Step 3: Dá»‹ch náº¿u cáº§n
            if (lang != "vi")
            {
                // Translate both fields in one request to reduce latency.
                var translated = await _translator.TranslateListAsync(new List<string> { viName, viDescription }, lang);
                if (translated != null && translated.Count >= 2)
                {
                    if (!string.IsNullOrWhiteSpace(translated[0])) translation.Name = translated[0];
                    if (!string.IsNullOrWhiteSpace(translated[1])) translation.Description = translated[1];
                }
            }

            if (isNew) _context.POITranslations.Add(translation);
            await _context.SaveChangesAsync();

            // Step 4: Táº¡o audio tá»« ná»™i dung Ä‘Ã£ dá»‹ch
            var textToSpeak = $"{translation.Name}. {translation.Description}";
            var audioUrl = await _tts.GenerateAudioAsync(poi.Id, lang, textToSpeak);

            if (!string.IsNullOrEmpty(audioUrl))
            {
                translation.AudioUrl = audioUrl;
                await _context.SaveChangesAsync();

                results.Add(new
                {
                    language = lang,
                    languageName = GetLangDisplayName(lang),
                    audioUrl,
                    name = translation.Name,
                    description = translation.Description,
                    success = true
                });
            }
            else
            {
                results.Add(new
                {
                    language = lang,
                    languageName = GetLangDisplayName(lang),
                    audioUrl = (string?)null,
                    name = translation.Name,
                    description = translation.Description,
                    success = false
                });
            }
        }

        return Ok(new
        {
            message = "ÄÃ£ táº¡o audio thuyáº¿t minh cho 3 ngÃ´n ngá»¯",
            results
        });
    }

    private static string GetLangDisplayName(string code) => code.ToLower() switch
    {
        "vi" => "Tiáº¿ng Viá»‡t",
        "en" => "English",
        "zh" => "ä¸­æ–‡ (Chinese)",
        _ => code.ToUpper()
    };
    }
}


