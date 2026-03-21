using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;
using PortfolioSite.Application.Services;
using System.Text.RegularExpressions;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public ContactController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.FullName) ||
            string.IsNullOrWhiteSpace(message.Email) ||
            string.IsNullOrWhiteSpace(message.Message))
            return BadRequest(new { error = "Tum alanlar zorunludur." });

        if (message.FullName.Length > 100)
            return BadRequest(new { error = "Ad soyad 100 karakterden uzun olamaz." });

        if (message.Email.Length > 200)
            return BadRequest(new { error = "Email adresi gecersiz." });

        if (message.Message.Length > 2000)
            return BadRequest(new { error = "Mesaj 2000 karakterden uzun olamaz." });

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(message.Email))
            return BadRequest(new { error = "Gecerli bir email adresi giriniz." });

        var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
       var recentMessage = await _context.ContactMessages
    .Where(m => m.Email == message.Email && m.SentAt > tenMinutesAgo)
    .Select(m => m.Id)
    .FirstOrDefaultAsync();

if (recentMessage != 0)
    return BadRequest(new { error = "Cok fazla mesaj gonderdiniz. Lutfen bekleyiniz." });
        message.FullName = message.FullName.Trim();
        message.Email = message.Email.Trim().ToLower();
        message.Message = message.Message.Trim();
     message.SentAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        message.IsRead = false;

        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync();

        // Onay emaili gönder — hata olursa mesaj yine de kaydedilsin
      try
{
    Console.WriteLine($"Email gönderiliyor: {message.Email}");
    await _emailService.SendConfirmationEmailAsync(message.Email, message.FullName);
    Console.WriteLine($"Email gönderildi!");
}
catch (Exception ex)
{
    Console.WriteLine($"Email gönderilemedi: {ex.Message}");
    Console.WriteLine($"Detay: {ex.InnerException?.Message}");
}

        return Ok(new { message = "Mesajiniz basariyla iletildi." });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var messages = await _context.ContactMessages
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
        return Ok(messages);
    }

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