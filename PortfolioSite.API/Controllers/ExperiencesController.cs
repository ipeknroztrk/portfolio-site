using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ExperiencesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExperiencesController(AppDbContext context)
    {
        _context = context;
    }

    // Public — herkes görebilir
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var experiences = await _context.Experiences
            .OrderBy(e => e.OrderIndex)
            .ToListAsync();

        return Ok(experiences);
    }

    // Admin — yeni deneyim ekle
   [Authorize]
[HttpPost]
public async Task<IActionResult> Create([FromBody] Experience experience)
{
    // PostgreSQL UTC datetime zorunluluğu
    experience.StartDate = DateTime.SpecifyKind(experience.StartDate, DateTimeKind.Utc);
    if (experience.EndDate.HasValue)
        experience.EndDate = DateTime.SpecifyKind(experience.EndDate.Value, DateTimeKind.Utc);

    _context.Experiences.Add(experience);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetAll), new { id = experience.Id }, experience);
}

    // Admin — deneyim güncelle
   [Authorize]
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, [FromBody] Experience experience)
{
    var existing = await _context.Experiences.FindAsync(id);
    if (existing == null) return NotFound();

    existing.Company = experience.Company;
    existing.Position = experience.Position;
    existing.Description = experience.Description;
    existing.StartDate = DateTime.SpecifyKind(experience.StartDate, DateTimeKind.Utc);
    existing.EndDate = experience.EndDate.HasValue
        ? DateTime.SpecifyKind(experience.EndDate.Value, DateTimeKind.Utc)
        : null;
    existing.IsCurrent = experience.IsCurrent;
    existing.OrderIndex = experience.OrderIndex;

    await _context.SaveChangesAsync();
    return Ok(existing);
}

    // Admin — deneyim sil
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var experience = await _context.Experiences.FindAsync(id);
        if (experience == null) return NotFound();

        _context.Experiences.Remove(experience);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}