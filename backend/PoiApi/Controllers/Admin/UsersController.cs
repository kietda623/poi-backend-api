using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        // Chỉ ADMIN mới được xem tất cả user
        [HttpGet]
        [Authorize(Roles = RoleConstants.Admin)]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {   
                    u.Id,
                    u.Email,
                    Role = u.Role.Name
                })
                .ToList();

            return Ok(users);
        }

        // GET: api/users/me
        // Lấy thông tin user đang đăng nhập
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == int.Parse(userId));

            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Email,
                Role = user.Role.Name
            });
        }

        [HttpPost("create-owner")]
        [Authorize(Roles = RoleConstants.Admin)]
        public IActionResult CreateOwner(CreateOwnerDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            var ownerRole = _context.Roles
                .FirstOrDefault(r => r.Name == RoleConstants.Owner);

            if (ownerRole == null)
                return StatusCode(500, "Owner role not found");

            var owner = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = ownerRole.Id
            };

            _context.Users.Add(owner);
            _context.SaveChanges();

            // (bước sau) tạo POI cho owner
            // var poi = new POI { Name = dto.ShopName, OwnerId = owner.Id }

            return Ok("Owner created successfully");
        }
    }
}
