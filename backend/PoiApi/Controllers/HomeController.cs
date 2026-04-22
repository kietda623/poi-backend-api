using Microsoft.AspNetCore.Mvc;

namespace PoiApi.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { message = "PoiApi backend is running." });
        }
    }
}
