using System.Net.Http.Json;
using System.Text.Json;
using AppUser.Models;

namespace AppUser.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly GuestService _guestService;
        private UserDto? _currentUser;
        private string? _token;

        // True nếu đang dùng Guest Token (ẩn danh)
        public bool IsGuest => _currentUser == null && _guestService.IsInitialized;
        public bool IsLoggedIn => _currentUser != null;
        public UserDto? CurrentUser => _currentUser;

        // Token: ưu tiên User Token, fallback sang Guest Token
        public string? Token => _token ?? _guestService.GuestToken;

        // Tự động phân giải localhost cho Android Emulator
        public static string BaseAddress =
            DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5279" : "http://localhost:5279";

        public AuthService(HttpClient httpClient, GuestService guestService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseAddress);
            _guestService = guestService;
        }

        /// <summary>
        /// Tự động khởi tạo Guest Session khi mở app (nếu chưa đăng nhập).
        /// </summary>
        public async Task InitGuestSessionAsync()
        {
            if (IsLoggedIn) return; // Đã đăng nhập, không cần Guest
            await _guestService.InitializeAsync();
        }

        public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { Email = email.Trim(), Password = password });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null)
                    {
                        _token = result.Token;
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                        _currentUser = new UserDto
                        {
                            Email = result.Email ?? email,
                            FullName = result.FullName ?? string.Empty,
                            Role = result.Role,
                            IsActive = true
                        };

                        // Bạn có thể lưu Token vào SecureStorage ở đây nếu muốn giữ trạng thái đăng nhập
                        // await SecureStorage.Default.SetAsync("auth_token", _token);

                        return (true, "Đăng nhập thành công");
                    }
                }
                try
                {
                    var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    if (error != null && error.ContainsKey("message"))
                        return (false, error["message"]);
                }
                catch { }

                return (false, "Email hoặc mật khẩu không đúng.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return (false, "Không thể kết nối đến máy chủ.");
            }
        }

        // Refresh user state from backend (important for admin block/disable).
        public async Task<(bool Success, string? Message)> RefreshMeAsync()
        {
            if (_token == null) return (false, "Chưa đăng nhập.");

            try
            {
                var response = await _httpClient.GetAsync("/api/auth/me");
                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return (false, !string.IsNullOrWhiteSpace(msg) ? msg : "Phiên đăng nhập không hợp lệ.");
                }

                var me = await response.Content.ReadFromJsonAsync<UserDto>();
                if (me == null) return (false, "Dữ liệu người dùng không hợp lệ.");

                _currentUser = me;
                return (true, null);
            }
            catch
            {
                return (false, "Không thể kết nối đến máy chủ.");
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string fullName)
        {
            try
            {
                var payload = new
                {
                    Email = email.Trim(),
                    Password = password,
                    FullName = fullName.Trim()
                };

                var response = await _httpClient.PostAsJsonAsync("/api/auth/register-user", payload);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Đăng ký thành công.");
                }
                
                // Đọc thông báo lỗi từ backend
                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, !string.IsNullOrWhiteSpace(errorMsg) ? errorMsg : "Đăng ký thất bại.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                return (false, "Không thể kết nối đến máy chủ.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(string email, string fullName, string? currentPassword, string? newPassword)
        {
            if (_token == null) return (false, "Chưa đăng nhập.");
            
            try
            {
                var payload = new 
                {
                    Email = email.Trim(),
                    FullName = fullName.Trim(),
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword
                };

                var response = await _httpClient.PutAsJsonAsync("/api/auth/profile", payload);
                if (response.IsSuccessStatusCode)
                {
                    if (_currentUser != null)
                    {
                        _currentUser.FullName = payload.FullName;
                        _currentUser.Email = payload.Email;
                    }
                    return (true, "Cập nhật hồ sơ thành công!");
                }
                
                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, !string.IsNullOrWhiteSpace(errorMsg) ? errorMsg : "Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update profile error: {ex.Message}");
                return (false, "Không thể kết nối đến máy chủ.");
            }
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            // Không xóa Guest session khi logout - vẫn giữ Guest access
            // _guestService.ClearSession();
            return Task.CompletedTask;
        }

        public UserDto? GetCurrentUser() => _currentUser;

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string? FullName { get; set; }
            public string? Email { get; set; }
        }
    }
}
