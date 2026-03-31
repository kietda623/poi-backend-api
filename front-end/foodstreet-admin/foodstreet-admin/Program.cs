using foodstreet_admin.Components;
using foodstreet_admin.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// ── Razor Components ─────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── MudBlazor ────────────────────────────────────────────────────
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 300;
    config.SnackbarConfiguration.ShowTransitionDuration = 300;
});

// ── Authentication & Authorization ───────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "FoodStreet.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// ── HttpClient → API backend ──────────────────────────────────────
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5279/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("Self", client =>
{
    client.BaseAddress = new Uri("http://localhost:5158/");
});
// Scoped HttpClient dùng cho ApiService
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// ── App Services ─────────────────────────────────────────────────
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ServicePackageService>();
builder.Services.AddScoped<UsageHistoryService>();
builder.Services.AddSingleton<PendingLoginService>();

// ── Logging ──────────────────────────────────────────────────────
builder.Logging.AddConsole();

// ── Build ─────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ── Logout endpoint ───────────────────────────────────────────────
app.MapGet("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/auth/login");
});

// ── Login cookie endpoint (handles cookie outside Blazor render) ──
app.MapGet("/auth/finalize", async (HttpContext ctx, string t, PendingLoginService pending) =>
{
    var entry = pending.Consume(t);
    if (entry == null) return Results.Redirect("/auth/login");

    // 1. Sửa ID "0" thành "1" (Hoặc entry.Value.Id.ToString() nếu có)
    string userId = "1";

    // 2. Kiểm tra Role: Nếu không phải ADMIN thì tự động gắn chuẩn thẻ "OWNER"
    string userRole = entry.Value.Role == "ADMIN" ? "ADMIN" : "OWNER";

    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
        new(System.Security.Claims.ClaimTypes.Name,  entry.Value.Email),
        new(System.Security.Claims.ClaimTypes.Email, entry.Value.Email),
        new(System.Security.Claims.ClaimTypes.Role,  userRole),
    };

    var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);
    var authProps = new AuthenticationProperties
    {
        IsPersistent = entry.Value.RememberMe,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
    };
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

    // Chuyển hướng đúng trang
    string redirect = userRole == "ADMIN" ? "/admin/dashboard" : "/seller/dashboard";
    return Results.Redirect(redirect);
});

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();