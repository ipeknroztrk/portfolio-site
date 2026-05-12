using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;

namespace PortfolioSite.API.Controllers
{
    [ApiController]
    [Route("api/ping")]
    public class PingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Ping()
        {
            var canConnect = _context.Database.CanConnect();

            return Ok(new
            {
                status = "pong",
                database = canConnect
            });
        }
    }
}