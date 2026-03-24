[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public LeaderboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddScore([FromBody] Score score)
    {
        score.CreatedAt = DateTime.UtcNow;
        _context.Scores.Add(score);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetTop()
    {
        var scores = await _context.Scores
            .OrderByDescending(x => x.Points)
            .Take(10)
            .ToListAsync();

        return Ok(scores);
    }
}