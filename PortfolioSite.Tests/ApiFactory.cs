using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;
using PortfolioSite.Application.Interfaces;
using PortfolioSite.Application.Services;

namespace PortfolioSite.Tests;

public class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _now;
    public override long GetTimestamp() => _now.UtcTicks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public void Advance(TimeSpan by) => _now += by;
}

// Gerçek API pipeline'ı (rate limit, auth, middleware) + InMemory veritabanı
public class ApiFactory : WebApplicationFactory<Program>
{
    public ManualTimeProvider Time { get; } = new();
    private readonly string _dbName = Guid.NewGuid().ToString();
    private static int _ipCounter;

    public string EnvironmentName { get; init; } = "Production";
    public int? MaxActiveSessions { get; init; }
    public Dictionary<string, string?> Settings { get; init; } = new();

    static ApiFactory()
    {
        // Program.cs bunları zorunlu tutuyor; gerçek bir veritabanına bağlanılmaz
        Environment.SetEnvironmentVariable("DB_CONNECTION", "Host=unused.invalid");
        Environment.SetEnvironmentVariable("JWT_SECRET", "test-only-secret-key-0123456789-0123456789-abcdef");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        foreach (var (key, value) in Settings)
            builder.UseSetting(key, value);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);

            if (MaxActiveSessions is int max)
            {
                services.RemoveAll<GameSessionStore>();
                services.AddSingleton(new GameSessionStore(Time) { MaxActiveSessions = max });
            }

            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);
        });
    }

    // Her istemci farklı bir IP gibi davranır, böylece testler birbirinin rate limitini tüketmez
    public HttpClient CreateClientWithIp(string? ip = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Real-IP", ip ?? $"10.0.{Interlocked.Increment(ref _ipCounter) / 250}.{_ipCounter % 250 + 1}");
        return client;
    }

    public HttpClient CreateAdminClient()
    {
        var client = CreateClientWithIp();
        using var scope = Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<ITokenService>()
            .GenerateAccessToken(new AdminUser { Id = 1, Username = "admin" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public List<Score> AllScores()
    {
        using var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>().Scores.AsNoTracking().ToList();
    }

    public void SeedScore(string name, int points)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Scores.Add(new Score { PlayerName = name, Points = points, CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }
}

// Hata middleware'ini test etmek için
[ApiController]
[Route("api/test-throw")]
public class ThrowingController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new InvalidOperationException("secret internal detail");
}
