using PortfolioSite.Application.Services;

namespace PortfolioSite.Tests;

public class LeaderboardRulesTests
{
    [Theory]
    [InlineData("ipek", "ipek")]
    [InlineData("  İpek   Öztürk ", "İpek Öztürk")]
    [InlineData("Mustafa Emre İlhan", "Mustafa Emre İlhan")]
    [InlineData("Uğur 🚀", "Uğur 🚀")]
    [InlineData("12345678901234567890", "12345678901234567890")]
    public void NormalizePlayerName_accepts_valid_names(string input, string expected)
    {
        Assert.Equal(expected, LeaderboardRules.NormalizePlayerName(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901")]   // 21 karakter
    [InlineData("ip​ek")]              // sıfır genişlikli boşluk
    [InlineData("ipek‮")]              // sağdan sola yazım kontrolü
    [InlineData("ip\u0000ek")]
    [InlineData("ip\u0007ek")]
    public void NormalizePlayerName_rejects_invalid_names(string? input)
    {
        Assert.Null(LeaderboardRules.NormalizePlayerName(input));
    }

    [Fact]
    public void NormalizePlayerName_unifies_unicode_forms()
    {
        // "Ö" tek karakter vs. "O" + birleşik iki nokta
        Assert.Equal(LeaderboardRules.NormalizePlayerName("Öztürk"),
                     LeaderboardRules.NormalizePlayerName("Öztürk"));
    }

    [Fact]
    public void MaxAchievableScore_is_zero_without_elapsed_time()
    {
        Assert.Equal(0, LeaderboardRules.MaxAchievableScore(TimeSpan.Zero));
    }

    [Fact]
    public void MaxAchievableScore_grows_with_time_and_is_capped()
    {
        var at10s = LeaderboardRules.MaxAchievableScore(TimeSpan.FromSeconds(10));
        var at20s = LeaderboardRules.MaxAchievableScore(TimeSpan.FromSeconds(20));

        Assert.True(at10s > 0);
        Assert.True(at20s > at10s);
        Assert.True(at10s < 99_111);   // Rapordaki skor 10 saniyede imkansız
        Assert.Equal(LeaderboardRules.MaxPoints, LeaderboardRules.MaxAchievableScore(TimeSpan.FromHours(2)));
    }

    [Fact]
    public void MaxAchievableScore_matches_hand_computed_bound()
    {
        // 1s => floor(1000/300)+1 = 4 yıldız; kombo bonusu max(1, k/3) = 1,1,1,1 => 4 * 60
        Assert.Equal(240, LeaderboardRules.MaxAchievableScore(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void MaxAchievableScore_allows_real_leaderboard_scores_in_realistic_time()
    {
        // Mevcut gerçek en iyi skor (10256) birkaç dakikalık oyunda rahatça kabul edilmeli
        Assert.True(LeaderboardRules.MaxAchievableScore(TimeSpan.FromMinutes(1)) >= 10_256);
    }
}
