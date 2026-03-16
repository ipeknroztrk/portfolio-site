namespace PortfolioSite.Application.Entities;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string? GithubUrl { get; set; }
    public string TechStack { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}