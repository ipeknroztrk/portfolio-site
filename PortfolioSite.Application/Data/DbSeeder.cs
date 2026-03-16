using Microsoft.EntityFrameworkCore;
using PortfolioSite.Application.Entities;

namespace PortfolioSite.Application.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Eğer zaten admin varsa tekrar oluşturma — her başlatmada çalışır ama sadece bir kez ekler
        if (await context.AdminUsers.AnyAsync()) return;

        var admin = new AdminUser
        {
            Username = "admin",
            // Şifreyi düz metin olarak saklamak güvensiz — BCrypt ile hashliyoruz
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            CreatedAt = DateTime.UtcNow
        };

        context.AdminUsers.Add(admin);
        await context.SaveChangesAsync();
    }
}