using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _context;

    public ContactController(AppDbContext context)
    {
        _context = context;
    }

    // Public — ziyaretçi mesaj gönderir
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.FullName) ||
            string.IsNullOrWhiteSpace(message.Email) ||
            string.IsNullOrWhiteSpace(message.Message))
            return BadRequest("Tum alanlar zorunludur.");

        message.SentAt = DateTime.UtcNow;
        message.IsRead = false;

        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Mesajiniz basariyla iletildi." });
    }

    // Admin — gelen mesajları listele
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var messages = await _context.ContactMessages
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

    // Admin — mesajı okundu olarak işaretle
    [Authorize]
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return NotFound();

        message.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok(message);
    }

    // Admin — mesaj sil
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return NotFound();

        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}