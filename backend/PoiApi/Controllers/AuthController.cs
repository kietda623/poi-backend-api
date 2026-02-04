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
        var user = _context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Email == dto.Email);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized();

        var token = GenerateToken(user);
        return Ok(new { token, role = user.Role.Name });
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

    // Resgister endpoint
    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {
        if (dto.Role == RoleConstants.Owner)
            return BadRequest("Cannot register as OWNER role");

        if (_context.Users.Any(u => u.Email == dto.Email))
            return BadRequest("Email existed");

        var roleName = dto.Role.Trim().ToLower();

        var role = _context.Roles
            .FirstOrDefault(r => r.Name.ToLower() == roleName);

        if (role == null)
            return BadRequest("Role invalid");

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            RoleId = role.Id
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("Register successfully");
    }



}
