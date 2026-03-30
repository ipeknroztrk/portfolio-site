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