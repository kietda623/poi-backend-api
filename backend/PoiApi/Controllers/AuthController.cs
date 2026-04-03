using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PoiApi.Data;
using PoiApi.DTOs.Auth;
using PoiApi.Models;    
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // login endpoint
    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        var email = dto.Email?.Trim().ToLower();

        var user = _context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Email == email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized();

        var token = GenerateToken(user);
        return Ok(new { token, role = user.Role.Name, fullName = user.FullName, email = user.Email, userId = user.Id });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

        var user = _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound("User not found");

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            Role = user.Role.Name,
            user.IsActive,
            user.CreatedAt
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var userId)) return Unauthorized();

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound("User not found");

        if (!dto.Email.EndsWith("@example.com"))
        {
            return BadRequest("Email phải có đuôi @example.com");
        }

        if (user.Email != dto.Email && _context.Users.Any(u => u.Email == dto.Email && u.Id != userId))
        {
            return BadRequest("Email đã được sử dụng bởi người khác.");
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            if (string.IsNullOrEmpty(dto.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Mật khẩu hiện tại không đúng.");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        _context.SaveChanges();

        return Ok(new { message = "Cập nhật hồ sơ thành công." });
    }

    private string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name.ToUpper())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,                    
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Generic Register endpoint for Admin Web (Owner/Admin/User)
    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email and password are required");

        if (_context.Users.Any(u => u.Email == dto.Email))
            return BadRequest("Email đã được sử dụng");

        var requestedRole = string.IsNullOrWhiteSpace(dto.Role) ? "OWNER" : dto.Role.ToUpper();
        if (requestedRole != "OWNER" && requestedRole != "ADMIN" && requestedRole != "USER")
            requestedRole = "OWNER";

        var role = _context.Roles.FirstOrDefault(r => r.Name == requestedRole);
        if (role == null)
            return StatusCode(500, $"Role {requestedRole} not configured");

        var user = new User
        {
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = role.Id
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "Đăng ký thành công", userId = user.Id });
    }

    // Register endpoint for user app (role always = USER)
    [HttpPost("register-user")]
    public IActionResult RegisterUser(UserRegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email and password are required");

        if (_context.Users.Any(u => u.Email == dto.Email))
            return BadRequest("Email đã được sử dụng");

        var role = _context.Roles.FirstOrDefault(r => r.Name == "USER");
        if (role == null)
            return StatusCode(500, "Role USER not configured");

        var user = new User
        {
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName.Trim(),
            RoleId = role.Id
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(new { message = "Đăng ký thành công", userId = user.Id });
    }



}
