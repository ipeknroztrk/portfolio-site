using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.DTOs;
using PortfolioSite.Application.Entities;
using PortfolioSite.Application.Services;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly GameSessionStore _sessions;
    private readonly ILogger<LeaderboardController> _logger;

    // Aynı isim için eşzamanlı iki gönderimin iki ayrı satır açmasını engeller (tek instance)
    private static readonly SemaphoreSlim ScoreWriteLock = new(1, 1);

    public LeaderboardController(AppDbContext context, GameSessionStore sessions, ILogger<LeaderboardController> logger)
    {
        _context = context;
        _sessions = sessions;
        _logger = logger;
    }

    // Oyun başlarken çağrılır; skor gönderimi bu oturum id'si ile yapılmalı
    [HttpPost("session")]
    public IActionResult StartSession()
    {
        var sessionId = _sessions.Create();
        if (sessionId == null)
            return StatusCode(503, new { error = "Too many active game sessions, try again later" });

        return Ok(new { sessionId, expiresInSeconds = (int)GameSessionStore.SessionLifetime.TotalSeconds });
    }

    [HttpPost]
    [RequestSizeLimit(4096)]
    public async Task<IActionResult> AddOrUpdateScore([FromBody] SubmitScoreRequestDto request)
    {
        // Validasyon (oturumu tüketmeden önce, böylece geçersiz isim oturumu yakmaz)
        var playerName = LeaderboardRules.NormalizePlayerName(request.PlayerName);
        if (playerName == null)
            return BadRequest(new { error = "Invalid player name" });

        if (request.Points < 0)
            return BadRequest(new { error = "Points cannot be negative" });

        // Oturum: var olmalı, süresi dolmamış olmalı, tek kullanımlık
        if (string.IsNullOrEmpty(request.SessionId) || !_sessions.TryConsume(request.SessionId, out var elapsed))
            return BadRequest(new { error = "Invalid or expired game session" });

        // Skor, oturum süresi içinde oyun kurallarıyla ulaşılabilir olmalı
        if (request.Points > LeaderboardRules.MaxAchievableScore(elapsed))
        {
            _logger.LogWarning("Implausible score rejected: {Points} after {ElapsedSeconds:F0}s", request.Points, elapsed.TotalSeconds);
            return BadRequest(new { error = "Score rejected" });
        }

        await ScoreWriteLock.WaitAsync();
        try
        {
            // Mevcut skoru kontrol et (case-insensitive).
            // Karşılaştırma .NET'te yapılır: SQL lower() ile .NET ToLower() Türkçe karakterlerde
            // (İ/ı) farklı sonuç verebildiği için aynı isim yeni kayıt açabiliyordu.
            var nameLength = playerName.EnumerateRunes().Count();
            var existingScore = (await _context.Scores
                    .Where(s => s.PlayerName.Length == nameLength)
                    .OrderByDescending(s => s.Points)
                    .ToListAsync())
                .FirstOrDefault(s => string.Equals(s.PlayerName, playerName, StringComparison.OrdinalIgnoreCase));
            
            if (existingScore != null)
            {
                // Eğer yeni skor daha yüksekse güncelle
                if (request.Points > existingScore.Points)
                {
                    existingScore.Points = request.Points;
                    existingScore.CreatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Score updated: {PlayerName} - {Points}", playerName, request.Points);
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
                _context.Scores.Add(new Score
                {
                    PlayerName = playerName,
                    Points = request.Points,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                _logger.LogInformation("New score added: {PlayerName} - {Points}", playerName, request.Points);
                return Ok(new { success = true, message = "Score saved successfully", isNew = true });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving score");
            return StatusCode(500, new { error = "Failed to save score" });
        }
        finally
        {
            ScoreWriteLock.Release();
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
                .Take(Math.Clamp(limit, 1, 50))
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
    
    // Admin için - tüm skorları getir
    [Authorize]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllScores()
    {
        var scores = await _context.Scores
            .OrderByDescending(s => s.Points)
            .ToListAsync();
            
        return Ok(scores);
    }
    
    // Mevcut kullanıcının tüm kayıtlarını temizle (opsiyonel, sadece admin)
    [Authorize]
    [HttpDelete("clean/{playerName}")]
    public async Task<IActionResult> CleanDuplicateScores(string playerName)
    {
        var scores = await _context.Scores
            .Where(s => s.PlayerName.ToLower() == playerName.ToLower())
            .OrderByDescending(s => s.Points)
            .ToListAsync();
            
        if (scores.Count <= 1)
            return Ok(new { message = "No duplicates found" });
            
        // En yüksek skoru tut, diğerlerini sil
        var bestScore = scores.First();
        var duplicates = scores.Skip(1);
        
        _context.Scores.RemoveRange(duplicates);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = $"Cleaned {duplicates.Count()} duplicate scores", kept = bestScore.Points });
    }
}