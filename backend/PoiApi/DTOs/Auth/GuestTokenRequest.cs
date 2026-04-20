namespace PoiApi.DTOs.Auth;

/// <summary>
/// DTO cho endpoint guest-token.
/// App gửi DeviceId để nhận JWT Token ẩn danh.
/// </summary>
public class GuestTokenRequest
{
    /// <summary>
    /// ID thiết bị từ app (bắt buộc).
    /// Dùng để định danh và liên kết subscription khi guest mua gói.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// GUID tạm thời định danh khách (tùy chọn).
    /// Nếu không truyền, server sẽ tự sinh mới.
    /// </summary>
    public string? GuestId { get; set; }
}
