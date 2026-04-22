using System.Net.Http.Json;
using AppUser.Models;

namespace AppUser.Services
{
    public class AuthService
    {
        private const string UserTokenKey = "auth_user_token";
        private const string UserEmailKey = "auth_user_email";
        private const string UserFullNameKey = "auth_user_fullname";
        private const string UserRoleKey = "auth_user_role";

        private readonly HttpClient _httpClient;
        private readonly GuestService _guestService;
        private readonly SignalRService _signalRService;
        private UserDto? _currentUser;
        private string? _token;
        private bool _sessionInitialized;

        public bool IsGuest => _currentUser == null && _guestService.IsInitialized;
        public bool IsLoggedIn => _currentUser != null;
        public UserDto? CurrentUser => _currentUser;

        public string? Token => _token ?? _guestService.GuestToken;

        public AuthService(HttpClient httpClient, GuestService guestService, SignalRService signalRService)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(AppConfig.BaseDomain);
            AppConfig.ConfigureHttpClient(_httpClient);
            _guestService = guestService;
            _signalRService = signalRService;
        }

        public async Task InitGuestSessionAsync()
        {
            await EnsureSessionLoadedAsync();
            if (IsLoggedIn) return;
            await _guestService.InitializeAsync();
        }

        public async Task EnsureSessionLoadedAsync()
        {
            if (_sessionInitialized)
            {
                return;
            }

            _sessionInitialized = true;

            try
            {
                var savedToken = await SecureStorage.Default.GetAsync(UserTokenKey);
                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    return;
                }

                var savedEmail = await SecureStorage.Default.GetAsync(UserEmailKey);
                var savedFullName = await SecureStorage.Default.GetAsync(UserFullNameKey);
                var savedRole = await SecureStorage.Default.GetAsync(UserRoleKey);

                _token = savedToken;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                _currentUser = new UserDto
                {
                    Email = savedEmail ?? string.Empty,
                    FullName = savedFullName ?? string.Empty,
                    Role = savedRole ?? string.Empty,
                    IsActive = true
                };

                var (success, _) = await RefreshMeAsync();
                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine("Session refresh failed, keep local session data.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session restore error: {ex.Message}");
                await ClearUserSessionAsync();
            }
        }

        public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
        {
            await EnsureSessionLoadedAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { Email = email.Trim(), Password = password });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null)
                    {
                        _token = result.Token;
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

                        _currentUser = new UserDto
                        {
                            Email = result.Email ?? email,
                            FullName = result.FullName ?? string.Empty,
                            Role = result.Role,
                            IsActive = true
                        };

                        await PersistUserSessionAsync();
                        _guestService.ClearSession();

                        try
                        {
                            await _signalRService.ConnectAsync(_token);
                        }
                        catch (Exception signalREx)
                        {
                            System.Diagnostics.Debug.WriteLine($"SignalR connect warning: {signalREx.Message}");
                        }

                        return (true, "Dang nhap thanh cong");
                    }
                }

                try
                {
                    var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    if (error != null && error.ContainsKey("message"))
                    {
                        return (false, error["message"]);
                    }
                }
                catch
                {
                }

                return (false, "Email hoac mat khau khong dung.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                return (false, "Khong the ket noi den may chu.");
            }
        }

        public async Task<(bool Success, string? Message)> RefreshMeAsync()
        {
            await EnsureSessionLoadedAsync();
            if (_token == null) return (false, "Chua dang nhap.");

            try
            {
                var response = await _httpClient.GetAsync("/api/auth/me");
                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    return (false, !string.IsNullOrWhiteSpace(msg) ? msg : "Phien dang nhap khong hop le.");
                }

                var me = await response.Content.ReadFromJsonAsync<UserDto>();
                if (me == null) return (false, "Du lieu nguoi dung khong hop le.");

                _currentUser = me;
                await PersistUserSessionAsync();
                return (true, null);
            }
            catch
            {
                return (false, "Khong the ket noi den may chu.");
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
                    return (true, "Dang ky thanh cong.");
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, !string.IsNullOrWhiteSpace(errorMsg) ? errorMsg : "Dang ky that bai.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Register error: {ex.Message}");
                return (false, "Khong the ket noi den may chu.");
            }
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            string email,
            string fullName,
            string? currentPassword,
            string? newPassword)
        {
            await EnsureSessionLoadedAsync();
            if (_token == null) return (false, "Chua dang nhap.");

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

                    await PersistUserSessionAsync();

                    return (true, "Cap nhat ho so thanh cong!");
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, !string.IsNullOrWhiteSpace(errorMsg) ? errorMsg : "Cap nhat that bai.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update profile error: {ex.Message}");
                return (false, "Khong the ket noi den may chu.");
            }
        }

        public async Task LogoutAsync()
        {
            await ClearUserSessionAsync();

            try
            {
                await _signalRService.DisconnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR disconnect warning: {ex.Message}");
            }
        }

        public UserDto? GetCurrentUser() => _currentUser;

        private async Task PersistUserSessionAsync()
        {
            if (string.IsNullOrWhiteSpace(_token) || _currentUser == null)
            {
                return;
            }

            await SecureStorage.Default.SetAsync(UserTokenKey, _token);
            await SecureStorage.Default.SetAsync(UserEmailKey, _currentUser.Email ?? string.Empty);
            await SecureStorage.Default.SetAsync(UserFullNameKey, _currentUser.FullName ?? string.Empty);
            await SecureStorage.Default.SetAsync(UserRoleKey, _currentUser.Role ?? string.Empty);
        }

        private Task ClearUserSessionAsync()
        {
            _currentUser = null;
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            SecureStorage.Default.Remove(UserTokenKey);
            SecureStorage.Default.Remove(UserEmailKey);
            SecureStorage.Default.Remove(UserFullNameKey);
            SecureStorage.Default.Remove(UserRoleKey);
            return Task.CompletedTask;
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string? FullName { get; set; }
            public string? Email { get; set; }
        }
    }
}

