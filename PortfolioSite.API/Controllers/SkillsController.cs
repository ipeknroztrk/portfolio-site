using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SkillsController(AppDbContext context)
    {
        _context = context;
    }

    // Public — herkes görebilir
    [HttpGet]
    [ResponseCache(Duration = 300)] // 5 dakika cache
    public async Task<IActionResult> GetAll()
    {
        var skills = await _context.Skills
            .Where(s => s.IsVisible)
            .OrderBy(s => s.Category)
            .ThenBy(s => s.OrderIndex)
            .ToListAsync();

        return Ok(skills);
    }

    // Admin — yeni skill ekle
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Skill skill)
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = skill.Id }, skill);
    }

    // Admin — skill güncelle
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Skill skill)
    {
        var existing = await _context.Skills.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = skill.Name;
        existing.Category = skill.Category;
        existing.Level = skill.Level;
        existing.OrderIndex = skill.OrderIndex;
        existing.IsVisible = skill.IsVisible;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // Admin — skill sil
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill == null) return NotFound();

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}