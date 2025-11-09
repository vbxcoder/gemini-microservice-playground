using Microsoft.AspNetCore.Mvc;

namespace Transports.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransportsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Your Transport List");
        }
    }
}
