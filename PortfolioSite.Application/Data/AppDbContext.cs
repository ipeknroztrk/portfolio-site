using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.Application.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Experience> Experiences => Set<Experience>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
public DbSet<Score> Scores { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .Property(p => p.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<Skill>()
            .Property(s => s.Level)
            .HasDefaultValue(1);

        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(r => r.AdminUser)
            .WithMany()
            .HasForeignKey(r => r.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}