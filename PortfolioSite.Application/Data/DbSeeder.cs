using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.Application.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Admin zaten varsa dokunma
        if (await context.AdminUsers.AnyAsync()) return;

        // Şifreyi .env'den oku, yoksa varsayılan kullan
        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
            ?? "Admin123!";

        var admin = new AdminUser
        {
            Username = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        context.AdminUsers.Add(admin);
        await context.SaveChangesAsync();
    }
}