using ISocialBlog.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ISocialBlog.Controllers
{
    [ApiController]
    [Route("")]

    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
