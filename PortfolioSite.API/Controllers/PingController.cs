using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioSite.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class PingController : ControllerBase
    {
        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok("API ayakta");
        }
    }
}