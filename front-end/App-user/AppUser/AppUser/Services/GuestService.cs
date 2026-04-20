using System.Net.Http.Json;

namespace AppUser.Services;

/// <summary>
/// Service quản lý Guest Session (khách vãng lai).
/// Tự động sinh GuestId, lấy DeviceId, và gọi API để nhận Guest Token.
/// Cho phép khách truy cập app mà không cần đăng ký tài khoản.
/// </summary>
public class GuestService
{
    private readonly HttpClient _httpClient;
    private string? _guestId;
    private string? _guestToken;
    private string? _deviceId;

    // Keys lưu trữ trong SecureStorage
    private const string GuestIdKey = "guest_id";
    private const string GuestTokenKey = "guest_token";

    public bool IsInitialized => !string.IsNullOrWhiteSpace(_guestToken);
    public string? GuestId => _guestId;
    public string? GuestToken => _guestToken;
    public string? DeviceId => _deviceId;

    public GuestService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.BaseApiUrl.Replace("/api/", ""))
        };
    }

    /// <summary>
    /// Khởi tạo Guest Session:
    /// 1. Lấy hoặc sinh GuestId (GUID)
    /// 2. Lấy DeviceId từ thiết bị
    /// 3. Gọi API lấy Guest Token
    /// 4. Lưu Token vào SecureStorage
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            // Lấy DeviceId từ thiết bị
            _deviceId = GetDeviceId();

            // Kiểm tra đã có token trong SecureStorage chưa
            var savedToken = await SecureStorage.Default.GetAsync(GuestTokenKey);
            var savedGuestId = await SecureStorage.Default.GetAsync(GuestIdKey);

            if (!string.IsNullOrWhiteSpace(savedToken) && !string.IsNullOrWhiteSpace(savedGuestId))
            {
                _guestToken = savedToken;
                _guestId = savedGuestId;
                System.Diagnostics.Debug.WriteLine($"[GuestService] Đã khôi phục Guest session: {_guestId}");
                return true;
            }

            // Sinh GuestId mới
            _guestId = savedGuestId ?? Guid.NewGuid().ToString("N");

            // Gọi API lấy Guest Token
            var response = await _httpClient.PostAsJsonAsync("/api/auth/guest-token", new
            {
                DeviceId = _deviceId,
                GuestId = _guestId
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GuestTokenResponse>();
                if (result != null && !string.IsNullOrWhiteSpace(result.Token))
                {
                    _guestToken = result.Token;

                    // Lưu vào SecureStorage
                    await SecureStorage.Default.SetAsync(GuestTokenKey, _guestToken);
                    await SecureStorage.Default.SetAsync(GuestIdKey, _guestId);

                    System.Diagnostics.Debug.WriteLine($"[GuestService] Guest session đã khởi tạo: {_guestId}");
                    return true;
                }
            }

            System.Diagnostics.Debug.WriteLine("[GuestService] Không thể lấy Guest Token từ API");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GuestService] Lỗi khởi tạo: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Xóa Guest Session (khi user đăng nhập bằng tài khoản chính thức).
    /// </summary>
    public void ClearSession()
    {
        _guestToken = null;
        _guestId = null;
        SecureStorage.Default.Remove(GuestTokenKey);
        SecureStorage.Default.Remove(GuestIdKey);
    }

    /// <summary>
    /// Lấy DeviceId độc nhất cho thiết bị hiện tại.
    /// </summary>
    private static string GetDeviceId()
    {
        try
        {
            // Kết hợp platform + model + name để tạo ID duy nhất
            var platform = DeviceInfo.Current.Platform.ToString();
            var model = DeviceInfo.Current.Model ?? "unknown";
            var name = DeviceInfo.Current.Name ?? "device";
            return $"{platform}_{model}_{name}".Replace(" ", "_").ToLowerInvariant();
        }
        catch
        {
            return $"device_{Guid.NewGuid():N}";
        }
    }

    // DTO cho response từ API
    private class GuestTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string GuestId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ExpiresInDays { get; set; }
    }
}
