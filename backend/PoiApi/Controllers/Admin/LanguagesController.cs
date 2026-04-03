using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoiApi.Data;
using PoiApi.Models;

namespace PoiApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/languages")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class LanguagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LanguagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetLanguages()
        {
            var languages = _context.Languages.OrderBy(l => l.Name).ToList();
            return Ok(languages);
        }

        [HttpPost]
        public IActionResult CreateLanguage(Language language)
        {
            language.CreatedAt = DateTime.UtcNow;
            _context.Languages.Add(language);
            _context.SaveChanges();
            return Ok(language);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateLanguage(int id, Language updated)
        {
            var language = _context.Languages.Find(id);
            if (language == null) return NotFound();

            language.Code = updated.Code;
            language.Name = updated.Name;
            language.IsActive = updated.IsActive;

            _context.SaveChanges();
            return Ok(language);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteLanguage(int id)
        {
            var language = _context.Languages.Find(id);
            if (language == null) return NotFound();

            _context.Languages.Remove(language);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
