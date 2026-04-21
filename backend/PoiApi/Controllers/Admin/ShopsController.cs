using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.Models;
using PoiApi.Services;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/shops")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class ShopsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly QrCodeService _qrCodeService;

        public ShopsController(AppDbContext context, IWebHostEnvironment environment, QrCodeService qrCodeService)
        {
            _context = context;
            _environment = environment;
            _qrCodeService = qrCodeService;
        }

        private async Task<string?> EnsureQrCodeAsync(Shop shop)
        {
            if (!string.IsNullOrWhiteSpace(shop.QrCodeUrl))
                return shop.QrCodeUrl;

            var shopUrl = _qrCodeService.BuildShopUrl(shop.Id);
            var qrCodeUrl = await _qrCodeService.GenerateQrCodeAsync(shopUrl, shop.Id);
            if (!string.IsNullOrWhiteSpace(qrCodeUrl))
            {
                shop.QrCodeUrl = qrCodeUrl;
            }

            return shop.QrCodeUrl;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShops([FromQuery] int? ownerId)
        {
            var query = _context.Shops
                .Include(s => s.Owner)
                .Include(s => s.Category)
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .AsQueryable();

            if (ownerId.HasValue)
            {
                query = query.Where(s => s.OwnerId == ownerId.Value);
            }

            var shopEntities = await query
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            foreach (var shop in shopEntities)
            {
                await EnsureQrCodeAsync(shop);
            }

            await _context.SaveChangesAsync();

            var shops = shopEntities
                .Select(s =>
                {
                    var audioUrls = s.Poi?.Translations?
                        .Where(t => !string.IsNullOrWhiteSpace(t.AudioUrl))
                        .GroupBy(t => t.LanguageCode)
                        .ToDictionary(g => g.Key, g => ToAbsoluteAudioUrl(g.First().AudioUrl))
                        ?? new Dictionary<string, string>();

                    return new
                    {
                        s.Id,
                        s.Name,
                        s.Description,
                        s.Address,
                        Phone = "",
                        ImageUrl = s.Poi != null ? s.Poi.ImageUrl : "",
                        AudioUrl = audioUrls.Values.FirstOrDefault() ?? "",
                        AudioUrls = audioUrls,
                        AudioTranslations = s.Poi?.Translations?
                            .OrderBy(t => t.LanguageCode)
                            .Select(t => new AudioTranslationDto
                            {
                                LanguageCode = t.LanguageCode,
                                Name = t.Name,
                                Description = t.Description,
                                AudioUrl = ToAbsoluteAudioUrl(t.AudioUrl)
                            })
                            .ToList() ?? new List<AudioTranslationDto>(),
                        Category = s.Category != null ? s.Category.Name : "Mac dinh",
                        Status = s.IsActive ? "Active" : "Pending",
                        SellerId = s.OwnerId,
                        SellerName = s.Owner.FullName,
                        s.CreatedAt,
                        Latitude = s.Poi != null ? s.Poi.Latitude : 0.0,
                        Longitude = s.Poi != null ? s.Poi.Longitude : 0.0,
                        TotalListens = _context.UsageHistories.Count(u => u.ShopId == s.Id),
                        TotalViews = s.ViewCount,
                        Rating = 0.0,
                        QrCodeUrl = s.QrCodeUrl
                    };
                })
                .ToList();

            return Ok(shops);
        }

        [HttpGet("{id}/qr")]
        public async Task<IActionResult> GetQrCode(int id)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == id);
            if (shop == null)
            {
                return NotFound("Gian hang khong ton tai");
            }

            await EnsureQrCodeAsync(shop);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                shopId = shop.Id,
                shopName = shop.Name,
                qrCodeUrl = shop.QrCodeUrl,
                encodedUrl = _qrCodeService.BuildShopUrl(shop.Id)
            });
        }

        [HttpPost("{id}/regenerate-qr")]
        public async Task<IActionResult> RegenerateQrCode(int id)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.Id == id);
            if (shop == null)
            {
                return NotFound("Gian hang khong ton tai");
            }

            var shopUrl = _qrCodeService.BuildShopUrl(shop.Id);
            var qrCodeUrl = await _qrCodeService.GenerateQrCodeAsync(shopUrl, shop.Id);

            if (string.IsNullOrWhiteSpace(qrCodeUrl))
            {
                return StatusCode(500, "Khong the tao ma QR");
            }

            shop.QrCodeUrl = qrCodeUrl;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                shopId = shop.Id,
                qrCodeUrl,
                encodedUrl = shopUrl,
                message = "Tao lai ma QR thanh cong"
            });
        }

        [HttpDelete("{id}/audio/{languageCode}")]
        public async Task<IActionResult> DeleteShopAudio(int id, string languageCode)
        {
            var shop = await _context.Shops
                .Include(s => s.Poi)
                    .ThenInclude(p => p.Translations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shop == null)
            {
                return NotFound("Gian hang khong ton tai");
            }

            var translation = shop.Poi?.Translations?
                .FirstOrDefault(t => t.LanguageCode.ToLower() == languageCode.ToLower());

            if (translation == null)
            {
                return NotFound("Khong tim thay ban thuyet minh theo ngon ngu nay");
            }

            if (!string.IsNullOrWhiteSpace(translation.AudioUrl))
            {
                DeleteAudioFile(translation.AudioUrl);
                translation.AudioUrl = null;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Da xoa file audio" });
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> ApproveShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null) return NotFound("Gian hang khong ton tai");

            shop.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gian hang da duoc duyet" });
        }

        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> RejectShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null) return NotFound("Gian hang khong ton tai");

            shop.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Gian hang da bi tu choi/vo hieu hoa" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null) return NotFound("Gian hang khong ton tai");

            _context.Shops.Remove(shop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private void DeleteAudioFile(string audioUrl)
        {
            var sanitizedPath = audioUrl.Split('?', '#')[0].Trim();
            if (string.IsNullOrWhiteSpace(sanitizedPath))
            {
                return;
            }

            if (Uri.TryCreate(sanitizedPath, UriKind.Absolute, out var absoluteUri))
            {
                sanitizedPath = absoluteUri.AbsolutePath;
            }

            sanitizedPath = sanitizedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullWebRootPath = Path.GetFullPath(webRootPath);
            var fullAudioPath = Path.GetFullPath(Path.Combine(fullWebRootPath, sanitizedPath));

            if (!fullAudioPath.StartsWith(fullWebRootPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (System.IO.File.Exists(fullAudioPath))
            {
                System.IO.File.Delete(fullAudioPath);
            }
        }

        private string ToAbsoluteAudioUrl(string? audioUrl)
        {
            if (string.IsNullOrWhiteSpace(audioUrl))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(audioUrl, UriKind.Absolute, out _))
            {
                return audioUrl;
            }

            return $"{Request.Scheme}://{Request.Host}/{audioUrl.TrimStart('/')}";
        }

        private sealed class AudioTranslationDto
        {
            public string LanguageCode { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string AudioUrl { get; set; } = string.Empty;
        }
    }
}
