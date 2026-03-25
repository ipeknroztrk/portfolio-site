using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(AppDbContext context, ILogger<LeaderboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddOrUpdateScore([FromBody] Score score)
    {
        try
        {
            // Validasyon
            if (string.IsNullOrWhiteSpace(score.PlayerName))
                return BadRequest(new { error = "Player name is required" });
            
            if (score.Points < 0)
                return BadRequest(new { error = "Points cannot be negative" });
            
            // İsim düzenleme
            score.PlayerName = score.PlayerName.Trim();
            
            // Mevcut skoru kontrol et
            var existingScore = await _context.Scores
                .FirstOrDefaultAsync(s => s.PlayerName == score.PlayerName);
            
            if (existingScore != null)
            {
                // Eğer yeni skor daha yüksekse güncelle
                if (score.Points > existingScore.Points)
                {
                    existingScore.Points = score.Points;
                    existingScore.CreatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Score updated: {score.PlayerName} - {score.Points}");
                    return Ok(new { success = true, message = "Score updated successfully", isNew = false });
                }
                else
                {
                    // Skor daha düşükse güncelleme yapma
                    return Ok(new { success = true, message = "Score not high enough to update", isNew = false });
                }
            }
            else
            {
                // Yeni kullanıcı, skoru ekle
                score.CreatedAt = DateTime.UtcNow;
                _context.Scores.Add(score);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"New score added: {score.PlayerName} - {score.Points}");
                return Ok(new { success = true, message = "Score saved successfully", isNew = true });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving score");
            return StatusCode(500, new { error = "Failed to save score" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetScores([FromQuery] int limit = 10)
    {
        try
        {
            var scores = await _context.Scores
                .OrderByDescending(s => s.Points)
                .ThenBy(s => s.CreatedAt)
                .Take(Math.Min(limit, 50))
                .Select(s => new
                {
                    s.Id,
                    s.PlayerName,
                    s.Points,
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            return StatusCode(500, new { error = "Failed to get leaderboard" });
        }
    }
}