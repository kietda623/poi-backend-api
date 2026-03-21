using foodstreet_admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace foodstreet_admin.Services;

public class AuthService
{
    private readonly ApiService _api;
    private readonly TokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApiService api,
        TokenService tokenService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _api = api;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Đăng nhập: gọi API thật → nhận JWT → lưu token → tạo cookie session
    /// </summary>
    public async Task<(bool Success, string Message, string RedirectUrl)> LoginAsync(LoginRequest request)
    {
        try
        {
            var result = await _api.PostAsync<LoginRequest, LoginResponse>(
                "auth/login",
                request);

            if (result == null)
                return (false, "Email hoặc mật khẩu không đúng!", "");

            // Lưu JWT cho các API call tiếp theo
            _tokenService.SetToken(result.Token);

            // Map role từ back-end (Admin/Owner/User) sang role front-end
            var role = result.Role?.ToLower() switch
            {
                "admin"  => "Admin",
                "owner"  => "Seller",   // Owner = Seller trong front-end
                _        => result.Role ?? "User"
            };

            // Tạo cookie session cho Blazor auth
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "0"),
                new(ClaimTypes.Name,           request.Email),
                new(ClaimTypes.Email,          request.Email),
                new(ClaimTypes.Role,           role),
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
            };

            await _httpContextAccessor.HttpContext!
                .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            string redirect = role == "Admin" ? "/admin/dashboard" : "/seller/dashboard";
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

    /// <summary>
    /// Đăng ký tài khoản mới bằng API thật
    /// </summary>
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
                email    = request.Email,
                password = request.Password,
                role     = "user"   // default role khi register từ admin panel
            };

            var response = await _api.PostAsync<object, object>("auth/register", payload);
            // API trả về 200 OK với string hoặc object
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

    /// <summary>Đăng xuất — xóa cookie và JWT token</summary>
    public async Task LogoutAsync()
    {
        _tokenService.ClearToken();
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>Đổi mật khẩu (TODO: gọi API khi back-end có endpoint)</summary>
    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPwd, string newPwd)
    {
        await Task.Delay(300);
        // TODO: await _api.PostAsync<object,object>("auth/change-password", new { userId, currentPwd, newPwd });
        return (true, "Đổi mật khẩu thành công!");
    }
}
