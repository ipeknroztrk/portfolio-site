namespace PortfolioSite.Application.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int OrderIndex { get; set; }
    public bool IsVisible { get; set; } = true;
}