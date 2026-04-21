using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PoiApi.Data;
using PoiApi.Hubs;
using PoiApi.Mapping;
using PoiApi.Models;
using PoiApi.Services;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// DATABASE
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);

// SERVICES
builder.Services.AddHttpClient(); 
builder.Services.AddScoped<IPoiService, PoiService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<AzureSpeechService>();
builder.Services.AddScoped<AzureTranslationService>();
builder.Services.AddScoped<SubscriptionAccessService>();
builder.Services.AddScoped<PayOsService>();
builder.Services.AddScoped<GroqService>();
// Guest token service - Cấp JWT ẩn danh cho khách vãng lai
builder.Services.AddScoped<GuestTokenService>();
// QR Code generation service
builder.Services.AddScoped<QrCodeService>();
builder.Services.AddSingleton<IUserTrackerService, UserTrackerService>();
builder.Services.Configure<PayOsOptions>(builder.Configuration.GetSection("PayOS"));

// AUTH
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });
builder.Services.AddAuthorization();

// ← ADD THIS: CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

// MVC / SWAGGER / MAPPER
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddAutoMapper(typeof(MappingProfile));

// BUILD APP
var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseStaticFiles();

// MIDDLEWARE
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DatabaseSchemaBootstrapper.EnsureServicePackageSchemaAsync(context);
    if (!context.Roles.Any(r => r.Name == "ADMIN"))
        context.Roles.Add(new Role { Name = "ADMIN" });
    if (!context.Roles.Any(r => r.Name == "OWNER"))
        context.Roles.Add(new Role { Name = "OWNER" });
    context.SaveChanges();
    await DefaultServicePackageCatalog.SyncAsync(context);
}

app.UseAuthentication();
// Enforce IsActive for all authenticated requests (prevents blocked users from keeping old JWTs)
app.Use(async (context, next) =>
{
    var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
    if (!isAuthenticated)
    {
        await next();
        return;
    }

    var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
    // Avoid interfering with auth flows.
    if (path.StartsWith("/api/auth/login") ||
        path.StartsWith("/api/auth/register") ||
        path.StartsWith("/api/auth/register-user"))
    {
        await next();
        return;
    }

    var userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Guest (khách vãng lai) không có record trong bảng Users → bỏ qua kiểm tra IsActive
    if (userIdString != null && userIdString.StartsWith("guest:"))
    {
        await next();
        return;
    }

    if (!int.TryParse(userIdString, out var userId))
    {
        await next();
        return;
    }

    var db = context.RequestServices.GetRequiredService<AppDbContext>();
    var isActive = await db.Users
        .Where(u => u.Id == userId)
        .Select(u => u.IsActive)
        .FirstOrDefaultAsync();

    if (isActive != true)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ để được hỗ trợ.\"}");
        return;
    }

    await next();
});
app.UseAuthorization();
app.MapControllers();
app.MapHub<UserTrackerHub>("/hubs/user-tracker");
app.Run();
