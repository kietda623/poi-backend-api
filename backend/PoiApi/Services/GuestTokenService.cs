using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PoiApi.Services;

/// <summary>
/// Service tạo JWT Token ẩn danh cho Guest (khách vãng lai).
/// Token chứa GuestId (GUID), DeviceId, Role = GUEST.
/// Cho phép khách truy cập các tính năng cơ bản mà không cần tạo tài khoản.
/// </summary>
public class GuestTokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<GuestTokenService> _logger;

    // Claim type tùy chỉnh cho Guest
    public const string GuestIdClaimType = "guest_id";
    public const string DeviceIdClaimType = "device_id";
    public const string GuestRole = "GUEST";

    public GuestTokenService(IConfiguration config, ILogger<GuestTokenService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Tạo JWT Token ẩn danh cho Guest.
    /// Token hết hạn sau 30 ngày, tự động gia hạn khi gọi lại.
    /// </summary>
    /// <param name="deviceId">ID thiết bị từ app (DeviceInfo)</param>
    /// <param name="guestId">GUID tạm thời định danh khách (tùy chọn, tự sinh nếu null)</param>
    /// <returns>JWT Token string</returns>
    public string GenerateGuestToken(string deviceId, string? guestId = null)
    {
        // Nếu không truyền guestId, tự sinh GUID mới
        var resolvedGuestId = string.IsNullOrWhiteSpace(guestId)
            ? Guid.NewGuid().ToString("N")
            : guestId;

        var claims = new List<Claim>
        {
            // Dùng guestId làm NameIdentifier để nhất quán với user token
            new(ClaimTypes.NameIdentifier, $"guest:{resolvedGuestId}"),
            new(GuestIdClaimType, resolvedGuestId),
            new(DeviceIdClaimType, deviceId),
            new(ClaimTypes.Role, GuestRole)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30), // Guest token sống 30 ngày
            signingCredentials: creds
        );

        _logger.LogInformation(
            "Đã tạo Guest Token cho DeviceId={DeviceId}, GuestId={GuestId}",
            deviceId, resolvedGuestId);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Kiểm tra xem user hiện tại có phải Guest hay không (dựa trên ClaimsPrincipal).
    /// </summary>
    public static bool IsGuest(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        return user.IsInRole(GuestRole);
    }

    /// <summary>
    /// Lấy GuestId từ ClaimsPrincipal (trả về null nếu không phải Guest).
    /// </summary>
    public static string? GetGuestId(ClaimsPrincipal? user)
    {
        return user?.FindFirstValue(GuestIdClaimType);
    }

    /// <summary>
    /// Lấy DeviceId từ ClaimsPrincipal.
    /// </summary>
    public static string? GetDeviceId(ClaimsPrincipal? user)
    {
        return user?.FindFirstValue(DeviceIdClaimType);
    }
}
