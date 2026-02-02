using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoiApi.Data;

namespace PoiApi.Controllers
{
    [ApiController]
    [Route("api/users")]
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
        [Authorize(Roles = "admin")]
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
    }
}
