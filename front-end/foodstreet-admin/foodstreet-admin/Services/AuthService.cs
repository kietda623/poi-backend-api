using foodstreet_admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Json;

namespace foodstreet_admin.Services;

public class AuthService
{
    private readonly ApiService _api;
    private readonly TokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PendingLoginService _pending;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApiService api,
        TokenService tokenService,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        PendingLoginService pending,
        ILogger<AuthService> logger)
    {
        _api = api;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _pending = pending;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, string RedirectUrl)> LoginAsync(LoginRequest request)
    {
        try
        {
            var result = await _api.PostAsync<LoginRequest, LoginResponse>(
                "auth/login",
                request);

            if (result == null)
                return (false, "Email hoặc mật khẩu không đúng!", "");

            _tokenService.SetToken(result.Token);

            var role = result.Role?.ToUpper() switch
            {
                "ADMIN" => "ADMIN",
                "OWNER" => "OWNER",
                _ => result.Role?.ToUpper() ?? "USER"
            };

            // ---- ĐÃ SỬA: Dùng PendingLoginService thay vì HttpClient ----
            var token = _pending.Store(request.Email, role, request.RememberMe);

            // ---- ĐÃ SỬA: Trỏ về trạm /auth/finalize trong Program.cs ----
            string redirect = $"/auth/finalize?t={token}";

            return (true, "Đăng nhập thành công!", redirect);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Login API call failed");
            return (false, "Không thể kết nối đến server. Vui lòng thử lại!", "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginAsync unexpected error");
            return (false, "Đã xảy ra lỗi. Vui lòng thử lại!", "");
        }
    }
    

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return (false, "Mật khẩu xác nhận không khớp!");
        if (request.Password.Length < 6)
            return (false, "Mật khẩu phải ít nhất 6 ký tự!");

        try
        {
            var payload = new
            {
                email = request.Email,
                password = request.Password,
                role = string.IsNullOrEmpty(request.Role) ? "OWNER" : request.Role
            };

            await _api.PostAsync<object, object>("auth/register", payload);
            return (true, "Đăng ký thành công! Vui lòng đăng nhập.");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            return (false, "Email đã được sử dụng hoặc vai trò không hợp lệ!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterAsync failed");
            return (false, "Không thể kết nối đến server. Vui lòng thử lại!");
        }
    }

    public async Task LogoutAsync()
    {
        _tokenService.ClearToken();
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPwd, string newPwd)
    {
        await Task.Delay(300);
        return (true, "Đổi mật khẩu thành công!");
    }
}
public class PendingLoginService
{
    private readonly Dictionary<string, (string Email, string Role, bool RememberMe, DateTime Expires)> _pending = new();

    public string Store(string email, string role, bool rememberMe)
    {
        var token = Guid.NewGuid().ToString("N");
        _pending[token] = (email, role, rememberMe, DateTime.UtcNow.AddMinutes(2));
        return token;
    }

    public (string Email, string Role, bool RememberMe)? Consume(string token)
    {
        if (_pending.TryGetValue(token, out var entry) && entry.Expires > DateTime.UtcNow)
        {
            _pending.Remove(token);
            return (entry.Email, entry.Role, entry.RememberMe);
        }
        return null;
    }
}