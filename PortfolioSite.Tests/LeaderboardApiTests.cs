using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PortfolioSite.Application.Services;

namespace PortfolioSite.Tests;

public class LeaderboardApiTests : IDisposable
{
    private readonly ApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private static async Task<string> StartSessionAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/leaderboard/session", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("sessionId").GetString()!;
    }

    private static Task<HttpResponseMessage> SubmitAsync(HttpClient client, object payload) =>
        client.PostAsJsonAsync("/api/leaderboard", payload);

    // 1. Rapordaki istek birebir: oturum yok
    [Fact]
    public async Task Reported_exploit_payload_is_rejected()
    {
        var client = _factory.CreateClientWithIp();

        var response = await SubmitAsync(client, new { playerName = "test", points = 99111 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.AllScores());
    }

    // 2. Uydurma oturum id'si
    [Fact]
    public async Task Unknown_session_is_rejected()
    {
        var client = _factory.CreateClientWithIp();

        var response = await SubmitAsync(client, new { sessionId = new string('A', 64), playerName = "test", points = 10 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.AllScores());
    }

    // 3. Normal oyun akışı çalışmaya devam ediyor
    [Fact]
    public async Task Legitimate_game_is_saved_and_listed()
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(90));

        var response = await SubmitAsync(client, new { sessionId, playerName = "ipek", points = 1250 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await client.GetFromJsonAsync<JsonElement>("/api/leaderboard?limit=10");
        var entry = Assert.Single(list.EnumerateArray());
        Assert.Equal("ipek", entry.GetProperty("playerName").GetString());
        Assert.Equal(1250, entry.GetProperty("points").GetInt32());
    }

    // 4. Aynı oturumla tekrar gönderim
    [Fact]
    public async Task Replayed_session_is_rejected()
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(90));

        var first = await SubmitAsync(client, new { sessionId, playerName = "ipek", points = 500 });
        var replay = await SubmitAsync(client, new { sessionId, playerName = "ipek", points = 900 });
        var replayOtherName = await SubmitAsync(client, new { sessionId, playerName = "other", points = 500 });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, replayOtherName.StatusCode);
        var score = Assert.Single(_factory.AllScores());
        Assert.Equal(500, score.Points);
    }

    // 5. Süresi dolmuş oturum
    [Fact]
    public async Task Expired_session_is_rejected()
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(GameSessionStore.SessionLifetime + TimeSpan.FromSeconds(1));

        var response = await SubmitAsync(client, new { sessionId, playerName = "ipek", points = 100 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.AllScores());
    }

    // 6. Geçerli oturum ama süreye göre imkansız skor
    [Theory]
    [InlineData(0, 99111)]       // oturum açılır açılmaz
    [InlineData(10, 99111)]      // 10 saniye sonra
    [InlineData(600, 99111)]     // mutlak üst sınırın üstünde
    [InlineData(600, 50_001)]
    public async Task Implausible_score_is_rejected(int elapsedSeconds, int points)
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(elapsedSeconds));

        var response = await SubmitAsync(client, new { sessionId, playerName = "test", points });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.AllScores());
    }

    [Fact]
    public async Task Score_at_the_plausibility_bound_is_accepted_and_one_above_is_rejected()
    {
        var client = _factory.CreateClientWithIp();
        var elapsed = TimeSpan.FromSeconds(5);
        var bound = LeaderboardRules.MaxAchievableScore(elapsed);

        var s1 = await StartSessionAsync(client);
        var s2 = await StartSessionAsync(client);
        _factory.Time.Advance(elapsed);

        var over = await SubmitAsync(client, new { sessionId = s1, playerName = "a", points = bound + 1 });
        var atBound = await SubmitAsync(client, new { sessionId = s2, playerName = "a", points = bound });

        Assert.Equal(HttpStatusCode.BadRequest, over.StatusCode);
        Assert.Equal(HttpStatusCode.OK, atBound.StatusCode);
    }

    // 7. Geçersiz isim/negatif puan; oturum yanmaz
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901")]
    [InlineData("ip​ek")]
    public async Task Invalid_player_name_is_rejected_without_consuming_session(string name)
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(60));

        var bad = await SubmitAsync(client, new { sessionId, playerName = name, points = 100 });
        var good = await SubmitAsync(client, new { sessionId, playerName = "ipek", points = 100 });

        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        Assert.Equal(HttpStatusCode.OK, good.StatusCode);
    }

    [Fact]
    public async Task Negative_points_are_rejected()
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(60));

        var response = await SubmitAsync(client, new { sessionId, playerName = "ipek", points = -5 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_factory.AllScores());
    }

    [Fact]
    public async Task Out_of_range_points_value_is_rejected()
    {
        var client = _factory.CreateClientWithIp();
        var content = new StringContent("{\"sessionId\":\"x\",\"playerName\":\"ipek\",\"points\":99999999999}", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/leaderboard", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 8. Mass assignment: Id / CreatedAt istemciden alınmaz
    [Fact]
    public async Task Client_supplied_id_and_createdAt_are_ignored()
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(60));

        var response = await SubmitAsync(client, new { sessionId, playerName = "ipek", points = 100, id = 9999, createdAt = "2000-01-01T00:00:00Z" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var score = Assert.Single(_factory.AllScores());
        Assert.NotEqual(9999, score.Id);
        Assert.True(score.CreatedAt > new DateTime(2020, 1, 1));
    }

    // Mevcut davranış korunur: yüksek skor günceller, düşük skor güncellemez, isim büyük/küçük harf duyarsız
    [Fact]
    public async Task Existing_update_rules_are_preserved()
    {
        _factory.SeedScore("Ipek", 300);
        var client = _factory.CreateClientWithIp();

        var lower = await StartSessionAsync(client);
        var higher = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(60));

        Assert.Equal(HttpStatusCode.OK, (await SubmitAsync(client, new { sessionId = lower, playerName = "ipek", points = 200 })).StatusCode);
        Assert.Equal(300, Assert.Single(_factory.AllScores()).Points);

        Assert.Equal(HttpStatusCode.OK, (await SubmitAsync(client, new { sessionId = higher, playerName = "  ipek ", points = 400 })).StatusCode);
        Assert.Equal(400, Assert.Single(_factory.AllScores()).Points);
    }

    // 9. Hız sınırı: oturum açma ve skor gönderme
    [Fact]
    public async Task Session_creation_is_rate_limited()
    {
        var client = _factory.CreateClientWithIp();
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 11; i++)
            statuses.Add((await client.PostAsync("/api/leaderboard/session", null)).StatusCode);

        Assert.All(statuses.Take(10), s => Assert.Equal(HttpStatusCode.OK, s));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[10]);

        // Başka bir istemci etkilenmez
        var other = _factory.CreateClientWithIp();
        Assert.Equal(HttpStatusCode.OK, (await other.PostAsync("/api/leaderboard/session", null)).StatusCode);
    }

    [Fact]
    public async Task Score_submission_is_rate_limited()
    {
        var client = _factory.CreateClientWithIp();
        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 11; i++)
            statuses.Add((await SubmitAsync(client, new { sessionId = "nope", playerName = "x", points = 1 })).StatusCode);

        Assert.All(statuses.Take(10), s => Assert.Equal(HttpStatusCode.BadRequest, s));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[10]);
    }

    // İstek boyutu: Kestrel [RequestSizeLimit] ile 413 döner; TestServer bu sınırı uygulamaz,
    // bu yüzden burada sadece büyük isteğin kayda dönüşmediği doğrulanır.
    [Fact]
    public async Task Oversized_body_is_rejected()
    {
        var client = _factory.CreateClientWithIp();
        var sessionId = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(60));
        var huge = new string('a', 10_000);

        var response = await SubmitAsync(client, new { sessionId, playerName = huge, points = 1 });

        Assert.False(response.IsSuccessStatusCode);
        Assert.Empty(_factory.AllScores());
    }

    // Türkçe/büyük-küçük harf farkı olan aynı isim yeni satır açmamalı
    [Theory]
    [InlineData("Mustafa Emre İLHAN", "Mustafa Emre İlhan")]
    [InlineData("Ipek", "ipek")]
    [InlineData("ŞEYMA", "şeyma")]
    public async Task Same_name_with_different_case_updates_existing_row(string stored, string submitted)
    {
        _factory.SeedScore(stored, 1180);
        var client = _factory.CreateClientWithIp();
        var lower = await StartSessionAsync(client);
        var higher = await StartSessionAsync(client);
        _factory.Time.Advance(TimeSpan.FromSeconds(60));

        await SubmitAsync(client, new { sessionId = lower, playerName = submitted, points = 1140 });
        await SubmitAsync(client, new { sessionId = higher, playerName = submitted, points = 1250 });

        Assert.Equal(1250, Assert.Single(_factory.AllScores()).Points);
    }

    // 10. Okuma uç noktası
    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(1000)]
    public async Task Leaderboard_read_handles_any_limit(int limit)
    {
        _factory.SeedScore("a", 10);
        var client = _factory.CreateClientWithIp();

        var response = await client.GetAsync($"/api/leaderboard?limit={limit}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Admin uç noktaları artık token istiyor
    [Theory]
    [InlineData("GET", "/api/leaderboard/all")]
    [InlineData("DELETE", "/api/leaderboard/clean/ipek")]
    public async Task Admin_endpoints_require_authentication(string method, string url)
    {
        _factory.SeedScore("ipek", 10);
        _factory.SeedScore("ipek", 5);
        var anonymous = _factory.CreateClientWithIp();

        var response = await anonymous.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(2, _factory.AllScores().Count);
    }

    [Fact]
    public async Task Admin_endpoints_work_with_valid_token()
    {
        _factory.SeedScore("beko", 10);
        _factory.SeedScore("BEKO", 5);
        var admin = _factory.CreateAdminClient();

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/leaderboard/all")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.DeleteAsync("/api/leaderboard/clean/beko")).StatusCode);
        Assert.Equal(10, Assert.Single(_factory.AllScores()).Points);
    }

    // Hata detayları production'da istemciye sızmaz
    [Fact]
    public async Task Unhandled_errors_do_not_leak_details()
    {
        var client = _factory.CreateClientWithIp();

        var response = await client.GetAsync("/api/test-throw");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("secret internal detail", body);
        Assert.DoesNotContain("stackTrace", body);
    }
}
