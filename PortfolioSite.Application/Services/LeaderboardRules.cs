using System.Globalization;
using System.Text;

namespace PortfolioSite.Application.Services;

public static class LeaderboardRules
{
    public const int MaxPlayerNameLength = 20; // game.component.html maxlength ile aynı
    public const int MaxPoints = 50_000;       // Mutlak üst sınır (gerçek en iyi skor ~10k)

    // game.component.ts'deki kurallarla eşleşmeli:
    // spawnStar() en az 300ms aralıkla yıldız üretir, en yüksek yıldız 30 puan,
    // çift puan x2, kombo bonusu floor(combo / 3).
    private const double MinSpawnIntervalMs = 300;
    private const int MaxStarPoints = 30;
    private const int MaxPointMultiplier = 2;

    // İsmi temizler; geçersizse null döner
    public static string? NormalizePlayerName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var normalized = raw.Normalize(NormalizationForm.FormC);
        var sb = new StringBuilder(normalized.Length);
        var lastWasSpace = false;

        foreach (var rune in normalized.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune))
            {
                if (!lastWasSpace) sb.Append(' ');
                lastWasSpace = true;
                continue;
            }

            switch (Rune.GetUnicodeCategory(rune))
            {
                case UnicodeCategory.Control:
                case UnicodeCategory.Format:       // sıfır genişlikli karakterler vb.
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.OtherNotAssigned:
                case UnicodeCategory.Surrogate:
                    return null;
            }

            sb.Append(rune.ToString());
            lastWasSpace = false;
        }

        var name = sb.ToString().Trim();
        if (name.Length == 0 || name.Length > MaxPlayerNameLength) return null;
        return name;
    }

    // Verilen sürede oyun kurallarına göre teorik olarak alınabilecek en yüksek skor.
    // Her yıldızın altın + çift puan + kesintisiz kombo olduğunu varsayar (cömert üst sınır).
    public static int MaxAchievableScore(TimeSpan elapsed)
    {
        if (elapsed <= TimeSpan.Zero) return 0;

        var stars = (long)(elapsed.TotalMilliseconds / MinSpawnIntervalMs) + 1;
        long total = 0;

        for (long k = 1; k <= stars; k++)
        {
            total += MaxStarPoints * MaxPointMultiplier * Math.Max(1, k / 3);
            if (total >= MaxPoints) return MaxPoints;
        }

        return (int)total;
    }
}
