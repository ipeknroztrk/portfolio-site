using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using PortfolioSite.API;
using PortfolioSite.Application.Services;

namespace PortfolioSite.Tests;

public class GameSessionStoreTests
{
    [Fact]
    public void Session_can_be_consumed_only_once_under_heavy_concurrency()
    {
        var store = new GameSessionStore(new ManualTimeProvider());
        var id = store.Create()!;

        var successes = 0;
        Parallel.For(0, 200, new ParallelOptions { MaxDegreeOfParallelism = 32 }, i =>
        {
            if (store.TryConsume(id, out _)) Interlocked.Increment(ref successes);
        });

        Assert.Equal(1, successes);
    }

    [Fact]
    public void Session_ids_are_unique_and_unguessable_length()
    {
        var store = new GameSessionStore(new ManualTimeProvider());
        var ids = Enumerable.Range(0, 1000).Select(_ => store.Create()!).ToList();

        Assert.Equal(1000, ids.Distinct().Count());
        Assert.All(ids, id => Assert.Equal(64, id.Length)); // 256 bit
    }

    [Fact]
    public void Full_store_refuses_new_sessions_until_expired_ones_are_purged()
    {
        var time = new ManualTimeProvider();
        var store = new GameSessionStore(time) { MaxActiveSessions = 2 };

        Assert.NotNull(store.Create());
        Assert.NotNull(store.Create());
        Assert.Null(store.Create());

        time.Advance(GameSessionStore.SessionLifetime + TimeSpan.FromSeconds(1));
        Assert.NotNull(store.Create());
    }

    [Fact]
    public void Expired_session_is_rejected_and_also_removed()
    {
        var time = new ManualTimeProvider();
        var store = new GameSessionStore(time);
        var id = store.Create()!;
        time.Advance(GameSessionStore.SessionLifetime + TimeSpan.FromSeconds(1));

        Assert.False(store.TryConsume(id, out _));
        Assert.False(store.TryConsume(id, out _));
    }
}

public class ConcurrentSubmissionTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    public void Dispose() => _factory.Dispose();

    private static async Task<string> StartSessionAsync(HttpClient client)
    {
        var body = await (await client.PostAsync("/api/leaderboard/session", null)).Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("sessionId").GetString()!;
    }

    [Fact]
    public async Task Simultaneous_submissions_with_the_same_session_succeed_only_once()
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(60));

        // 8 istek (rate limit 10/dk) aynı anda, farklı isim ve puanlarla
        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(i =>
            client.PostAsJsonAsync("/api/leaderboard", new { sessionId, playerName = $"p{i}", points = 100 + i })));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(7, responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest));
        Assert.Single(_factory.AllScores());
    }

    [Fact]
    public async Task Simultaneous_first_submissions_for_the_same_name_create_one_row()
    {
        var sessions = new List<(HttpClient Client, string Id)>();
        for (var i = 0; i < 8; i++)
        {
            var c = _factory.CreateClientWithIp();
            sessions.Add((c, await StartSessionAsync(c)));
        }
        _factory.Time.Advance(TimeSpan.FromSeconds(60));

        var responses = await Task.WhenAll(sessions.Select((s, i) =>
            s.Client.PostAsJsonAsync("/api/leaderboard", new { sessionId = s.Id, playerName = "ipek", points = 100 + i })));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var row = Assert.Single(_factory.AllScores());
        Assert.Equal(107, row.Points);
    }
}

public class SessionCapacityTests : IDisposable
{
    private readonly ApiFactory _factory = new() { MaxActiveSessions = 1 };
    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Session_creation_failure_returns_503_and_score_cannot_be_submitted_without_session()
    {
        var client = _factory.CreateClientWithIp();
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/leaderboard/session", null)).StatusCode);

        var failed = await client.PostAsync("/api/leaderboard/session", null);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, failed.StatusCode);

        // Oturum alınamayan istemci eski (oturumsuz) biçime dönse bile reddedilir
        var legacy = await client.PostAsJsonAsync("/api/leaderboard", new { playerName = "ipek", points = 100 });
        var nullSession = await client.PostAsJsonAsync("/api/leaderboard", new { sessionId = (string?)null, playerName = "ipek", points = 100 });
        Assert.Equal(HttpStatusCode.BadRequest, legacy.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, nullSession.StatusCode);
        Assert.Empty(_factory.AllScores());
    }
}

public class SwaggerExposureTests
{
    [Fact]
    public async Task Swagger_is_not_served_in_production()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClientWithIp();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger/index.html")).StatusCode);
    }

    [Fact]
    public async Task Swagger_is_still_served_in_development()
    {
        using var factory = new ApiFactory { EnvironmentName = "Development" };
        var client = factory.CreateClientWithIp();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode);
    }
}

public class JwtSecretTests
{
    private const string PlaceholderKey = "super-secret-key";
    private const string StrongKey = "0123456789abcdef0123456789abcdef-strong";

    private sealed class Env(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "test";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static IConfiguration Config(string? envSecret, string? appsettingsSecret) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JWT_SECRET"] = envSecret,
            ["JwtSettings:SecretKey"] = appsettingsSecret
        }).Build();

    [Fact]
    public void Production_without_JWT_SECRET_fails_and_does_not_use_appsettings_value()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            JwtSecretResolver.Resolve(Config(null, StrongKey), new Env("Production")));

        Assert.Contains("JWT_SECRET is not set", ex.Message);
        Assert.DoesNotContain(StrongKey, ex.Message);
    }

    [Fact]
    public void Placeholder_key_is_rejected_even_in_development()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            JwtSecretResolver.Resolve(Config(null, PlaceholderKey), new Env("Development")));

        Assert.Contains("too short", ex.Message);
        Assert.DoesNotContain(PlaceholderKey, ex.Message);
    }

    [Fact]
    public void Short_JWT_SECRET_is_rejected_in_production()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            JwtSecretResolver.Resolve(Config(PlaceholderKey, null), new Env("Production")));

        Assert.DoesNotContain(PlaceholderKey, ex.Message);
    }

    [Fact]
    public void Valid_JWT_SECRET_is_used_and_preferred()
    {
        Assert.Equal(StrongKey, JwtSecretResolver.Resolve(Config(StrongKey, "other-value-that-is-long-enough-000000"), new Env("Production")));
    }

    [Fact]
    public void Development_may_fall_back_to_a_strong_appsettings_key()
    {
        Assert.Equal(StrongKey, JwtSecretResolver.Resolve(Config(null, StrongKey), new Env("Development")));
    }

    [Fact]
    public void Api_does_not_start_in_production_when_JWT_SECRET_is_empty()
    {
        using var factory = new ApiFactory { Settings = { ["JWT_SECRET"] = "" } };

        var ex = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        var inner = ex as InvalidOperationException ?? ex.InnerException as InvalidOperationException;
        Assert.NotNull(inner);
        Assert.Contains("JWT_SECRET is not set", inner!.Message);
    }
}
