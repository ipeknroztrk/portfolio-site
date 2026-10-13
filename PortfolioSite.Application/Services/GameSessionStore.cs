using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace PortfolioSite.Application.Services;

// Sunucunun verdiği tek kullanımlık oyun oturumları. Skor sadece geçerli bir oturumla kaydedilebilir.
// Bellekte tutulur: API yeniden başlarsa açık oturumlar düşer (o oyunun skoru kaydedilmez).
public class GameSessionStore
{
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(2);

    // Bellek koruması: dolarsa yeni oturum verilmez (503). Kayıt başına ~200 bayt.
    public int MaxActiveSessions { get; init; } = 50_000;

    // Değer: oturumun açıldığı monotonik zaman damgası (sistem saati ayarlarından etkilenmez)
    private readonly ConcurrentDictionary<string, long> _sessions = new();
    private readonly TimeProvider _time;

    public GameSessionStore(TimeProvider time)
    {
        _time = time;
    }

    // Yeni oturum id'si döner; çok fazla açık oturum varsa null
    public string? Create()
    {
        if (_sessions.Count >= MaxActiveSessions)
        {
            PurgeExpired();
            if (_sessions.Count >= MaxActiveSessions) return null;
        }

        var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _sessions[id] = _time.GetTimestamp();
        return id;
    }

    // Oturumu tüketir (tekrar kullanılamaz). Geçerliyse oturum açıldığından beri geçen süreyi döner.
    // TryRemove atomiktir: aynı oturumla eşzamanlı iki istekten sadece biri başarılı olur.
    public bool TryConsume(string sessionId, out TimeSpan elapsed)
    {
        elapsed = TimeSpan.Zero;
        if (!_sessions.TryRemove(sessionId, out var issuedAt)) return false;

        elapsed = _time.GetElapsedTime(issuedAt);
        return elapsed >= TimeSpan.Zero && elapsed <= SessionLifetime;
    }

    private void PurgeExpired()
    {
        foreach (var (id, issuedAt) in _sessions)
        {
            if (_time.GetElapsedTime(issuedAt) > SessionLifetime)
                _sessions.TryRemove(id, out _);
        }
    }
}
