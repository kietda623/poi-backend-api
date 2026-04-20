using QRCoder;

namespace PoiApi.Services
{
    /// <summary>
    /// Generates QR code PNG files for shop URLs and saves them to wwwroot/qr/.
    /// Uses QRCoder's PngByteQRCode which is cross-platform (no System.Drawing dependency).
    /// </summary>
    public class QrCodeService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger<QrCodeService> _logger;

        public QrCodeService(IWebHostEnvironment env, IConfiguration config, ILogger<QrCodeService> logger)
        {
            _env = env;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Generates a QR code PNG for the given URL and saves it to wwwroot/qr/.
        /// Returns the relative URL path (e.g. "/qr/shop-5-qr.png") or null on failure.
        /// </summary>
        public async Task<string?> GenerateQrCodeAsync(string url, int shopId)
        {
            try
            {
                var qrDir = Path.Combine(_env.WebRootPath, "qr");
                Directory.CreateDirectory(qrDir);

                var fileName = $"shop-{shopId}-qr.png";
                var filePath = Path.Combine(qrDir, fileName);

                // Generate QR code as PNG bytes (no System.Drawing needed)
                var qrGenerator = new QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrData);

                // Generate with: white background, dark foreground (orange brand color)
                byte[] pngBytes = qrCode.GetGraphic(
                    pixelsPerModule: 10,
                    darkColorRgba: new byte[] { 0xFF, 0x6B, 0x35, 0xFF }, // #FF6B35 orange
                    lightColorRgba: new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }  // white
                );

                await File.WriteAllBytesAsync(filePath, pngBytes);

                _logger.LogInformation("QR code generated for shop {ShopId}: {FilePath}", shopId, filePath);
                return $"/qr/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate QR code for shop {ShopId}", shopId);
                return null;
            }
        }

        /// <summary>
        /// Builds the public URL that gets encoded into the QR code.
        /// Format: {AppBaseUrl}/poi/{shopId}  →  mobile app deep link.
        /// </summary>
        public string BuildShopUrl(int shopId)
        {
            var baseUrl = _config["App:BaseUrl"] ?? "https://foodstreet.app";
            return $"{baseUrl}/poi/{shopId}";
        }
    }
}
