using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProjectsController(AppDbContext context)
    {
        _context = context;
    }

    // Public — herkes görebilir
   [HttpGet]
[ResponseCache(Duration = 300)] // 5 dakika cache
public async Task<IActionResult> GetAll()
{
    var projects = await _context.Projects
        .Where(p => p.IsVisible)
        .OrderBy(p => p.OrderIndex)
        .ToListAsync();
    return Ok(projects);
}

    // Sadece admin görebilir — tüm projeler (görünmeyenler dahil)
   [Authorize]
[HttpGet("all")]
[ResponseCache(NoStore = true, Duration = 0)]
public async Task<IActionResult> GetAllAdmin()
{
    var projects = await _context.Projects
        .OrderBy(p => p.OrderIndex)
        .ToListAsync();
    return Ok(projects);
}

    // Admin — yeni proje ekle
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Project project)
    {
        project.CreatedAt = DateTime.UtcNow;
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = project.Id }, project);
    }

    // Admin — proje güncelle
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Project project)
    {
        var existing = await _context.Projects.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = project.Title;
        existing.Description = project.Description;
        existing.ImageUrl = project.ImageUrl;
        existing.LiveUrl = project.LiveUrl;
        existing.GithubUrl = project.GithubUrl;
        existing.TechStack = project.TechStack;
        existing.IsVisible = project.IsVisible;
        existing.OrderIndex = project.OrderIndex;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    // Admin — proje sil
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}