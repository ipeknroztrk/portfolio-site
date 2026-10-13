using System.Text;

namespace PortfolioSite.API;

// JWT anahtarını güvenli şekilde çözer. Production'da sadece JWT_SECRET ortam değişkeni kabul edilir;
// appsettings'teki değer yalnızca Development'ta yedek olarak kullanılır. Hata mesajları anahtarı içermez.
public static class JwtSecretResolver
{
    public const int MinSecretBytes = 32; // HS256 için en az 256 bit

    public static string Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var secret = configuration["JWT_SECRET"];
        var source = "JWT_SECRET environment variable";

        if (string.IsNullOrWhiteSpace(secret) && environment.IsDevelopment())
        {
            secret = configuration["JwtSettings:SecretKey"];
            source = "JwtSettings:SecretKey (Development fallback)";
        }

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException(
                $"JWT_SECRET is not set. Set the JWT_SECRET environment variable to a random value of at least {MinSecretBytes} bytes " +
                $"(environment: {environment.EnvironmentName}). The appsettings fallback is only allowed in Development.");

        if (Encoding.UTF8.GetByteCount(secret) < MinSecretBytes)
            throw new InvalidOperationException(
                $"The JWT secret from {source} is too short: at least {MinSecretBytes} bytes are required for HS256.");

        return secret;
    }
}
