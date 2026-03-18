using foodstreet_admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace foodstreet_admin.Services;

public class AuthService
{
    private readonly ApiService _api;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApiService api, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
    {
        _api = api;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Đăng nhập: gọi API → nhận token → tạo cookie session
    /// TODO: Thay mock bằng _api.PostAsync<LoginRequest, AuthResponse>("/auth/login", request)
    /// </summary>
    public async Task<(bool Success, string Message, string RedirectUrl)> LoginAsync(LoginRequest request)
    {
        // ── MOCK (xóa khi có API thật) ──────────────────────────
        await Task.Delay(500);
        UserModel? user = null;

        if (request.Email == "admin@foodstreet.vn" && request.Password == "admin123")
            user = new UserModel { Id=1, FullName="Admin", Email=request.Email, Role="Admin" };
        else if (request.Email == "seller@foodstreet.vn" && request.Password == "seller123")
            user = new UserModel { Id=2, FullName="Nguyễn Văn A", Email=request.Email, Role="Seller" };

        if (user == null) return (false, "Email hoặc mật khẩu không đúng!", "");
        // ────────────────────────────────────────────────────────

        /* ── PRODUCTION: uncomment block này ─────────────────────
        var result = await _api.PostAsync<LoginRequest, AuthResponse>("/auth/login", request);
        if (result == null || !result.Success) return (false, result?.Message ?? "Đăng nhập thất bại", "");
        var user = result.User!;
        ─────────────────────────────────────────────────────────── */

        // Tạo claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        var ctx = _httpContextAccessor.HttpContext!;
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

        string redirect = user.Role == "Admin" ? "/admin/dashboard" : "/seller/dashboard";
        return (true, "Đăng nhập thành công!", redirect);
    }

    /// <summary>
    /// Đăng ký tài khoản Seller mới
    /// TODO: Thay mock bằng _api.PostAsync<RegisterRequest, AuthResponse>("/auth/register", request)
    /// </summary>
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        await Task.Delay(500);

        // Validate
        if (request.Password != request.ConfirmPassword)
            return (false, "Mật khẩu xác nhận không khớp!");
        if (request.Password.Length < 6)
            return (false, "Mật khẩu phải ít nhất 6 ký tự!");

        // TODO: gọi API thật
        // var result = await _api.PostAsync<RegisterRequest, AuthResponse>("/auth/register", request);
        // return (result?.Success ?? false, result?.Message ?? "Đăng ký thất bại");

        return (true, "Đăng ký thành công! Vui lòng đăng nhập.");
    }

    /// <summary>Đăng xuất</summary>
    public async Task LogoutAsync()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>Đổi mật khẩu</summary>
    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPwd, string newPwd)
    {
        await Task.Delay(300);
        // TODO: _api.PostAsync("/auth/change-password", new { userId, currentPwd, newPwd })
        return (true, "Đổi mật khẩu thành công!");
    }
}
