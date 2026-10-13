using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.Entities;
using PortfolioSite.Application.Services;

namespace PortfolioSite.Tests;

public class ExperienceOrderingTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    public void Dispose() => _factory.Dispose();

    private void Seed(params Experience[] experiences)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Experiences.AddRange(experiences);
        db.SaveChanges();
    }

    private static Experience Exp(string company, string position, DateTime start, bool current = false, int orderIndex = 0) =>
        new() { Company = company, Position = position, StartDate = start, IsCurrent = current, OrderIndex = orderIndex, Description = "" };

    private async Task<List<string>> GetOrderAsync()
    {
        var list = await _factory.CreateClientWithIp().GetFromJsonAsync<JsonElement>("/api/v1/experiences");
        return list.EnumerateArray()
            .Select(e => $"{e.GetProperty("company").GetString()} | {e.GetProperty("position").GetString()}")
            .ToList();
    }

    [Fact]
    public async Task Current_first_then_newest_start_date_regardless_of_OrderIndex_or_insert_order()
    {
        // OrderIndex ve ekleme sırası kasıtlı olarak karışık
        Seed(
            Exp("Vakıfbank", "Full Stack Developer Intern", new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc), orderIndex: 1),
            Exp("TEKNOFEST", "Yazılım Geliştirici Üyesi", new DateTime(2025, 2, 22, 0, 0, 0, DateTimeKind.Utc), orderIndex: 5),
            Exp("Vakıfbank", ".NET Developer Intern", new DateTime(2023, 7, 1, 0, 0, 0, DateTimeKind.Utc), orderIndex: 2),
            Exp("SEV – Sağlık ve Eğitim Vakfı", "Yazılım Uzman Yardımcısı", new DateTime(2026, 8, 31, 21, 0, 0, DateTimeKind.Utc), current: true, orderIndex: 4),
            Exp("Uyumsoft", "Software Developer Intern", new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc), orderIndex: 3));

        Assert.Equal(new[]
        {
            "SEV – Sağlık ve Eğitim Vakfı | Yazılım Uzman Yardımcısı",
            "Uyumsoft | Software Developer Intern",
            "TEKNOFEST | Yazılım Geliştirici Üyesi",
            "Vakıfbank | Full Stack Developer Intern",
            "Vakıfbank | .NET Developer Intern",
        }, await GetOrderAsync());
    }

    [Fact]
    public async Task Current_experience_is_first_even_if_it_started_earlier()
    {
        Seed(
            Exp("Newer", "Past job", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            Exp("Older", "Current job", new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), current: true));

        Assert.Equal(new[] { "Older | Current job", "Newer | Past job" }, await GetOrderAsync());
    }

    [Fact]
    public async Task Same_start_date_falls_back_to_OrderIndex()
    {
        var date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Seed(Exp("B", "x", date, orderIndex: 2), Exp("A", "x", date, orderIndex: 1));

        Assert.Equal(new[] { "A | x", "B | x" }, await GetOrderAsync());
    }
}

public class GameplayCompatibilityTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    public void Dispose() => _factory.Dispose();

    // Rate limit kuralları (session + submit) gerçek oyun temposunu engellememeli:
    // bir oyuncu dakikada 5 oyun bitirebilmeli.
    [Fact]
    public async Task Five_complete_games_per_minute_from_one_ip_succeed()
    {
        var client = _factory.CreateClientWithIp();

        for (var i = 0; i < 5; i++)
        {
            var session = await client.PostAsync("/api/leaderboard/session", null);
            Assert.Equal(HttpStatusCode.OK, session.StatusCode);
            var sessionId = (await session.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("sessionId").GetString();

            _factory.Time.Advance(TimeSpan.FromSeconds(10));
            var submit = await client.PostAsJsonAsync("/api/leaderboard", new { sessionId, playerName = "ipek", points = 100 + i });
            Assert.Equal(HttpStatusCode.OK, submit.StatusCode);
        }
    }

    // Soğuk başlangıç: oturum, oyun başladıktan sonra verilebilir (API uyanırken).
    // Sunucunun ölçtüğü süre kısa kalsa bile gerçekçi skorlar kabul edilmeli.
    [Theory]
    [InlineData(5, 1_000)]     // ~5s ölçülen süre, iyi bir oyun
    [InlineData(10, 10_256)]   // şu anki en iyi gerçek skor
    public void Realistic_scores_fit_even_when_server_measured_time_is_short(int measuredSeconds, int score)
    {
        Assert.True(score <= LeaderboardRules.MaxAchievableScore(TimeSpan.FromSeconds(measuredSeconds)));
    }

    // Saat geri alınsa bile (NTP vb.) monotonik zaman kullanıldığı için oturum bozulmaz
    [Fact]
    public void Session_elapsed_time_uses_monotonic_timestamp()
    {
        var time = new ManualTimeProvider();
        var store = new GameSessionStore(time);
        var id = store.Create()!;
        time.Advance(TimeSpan.FromSeconds(42));

        Assert.True(store.TryConsume(id, out var elapsed));
        Assert.Equal(TimeSpan.FromSeconds(42), elapsed);
    }
}

public class RateLimitRuleMatchingTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    public void Dispose() => _factory.Dispose();

    // Oturum istekleri skor gönderim kotasını tüketmemeli (kurallar ayrı sayılır)
    [Fact]
    public async Task Session_requests_do_not_consume_score_submission_quota()
    {
        var client = _factory.CreateClientWithIp();
        string? lastSession = null;
        for (var i = 0; i < 10; i++)
        {
            var r = await client.PostAsync("/api/leaderboard/session", null);
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);
            lastSession = (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("sessionId").GetString();
        }
        _factory.Time.Advance(TimeSpan.FromSeconds(30));

        var submit = await client.PostAsJsonAsync("/api/leaderboard", new { sessionId = lastSession, playerName = "ipek", points = 100 });

        Assert.Equal(HttpStatusCode.OK, submit.StatusCode);
    }
}
