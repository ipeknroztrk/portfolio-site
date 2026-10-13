using System.Text;
using AspNetCoreRateLimit;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioSite.API;
using PortfolioSite.API.Middleware;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.DTOs;
using PortfolioSite.Application.Interfaces;
using PortfolioSite.Application.Services;

// .env dosyasını yükle
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

builder.Services.AddControllers();

// Response caching
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

// Rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    // İstemci IP'si yalnızca proxy'nin (Render/Cloudflare) yazdığı başlıktan okunur; X-Forwarded-For
    // kullanılmaz (Render ekler, temizlemez → ilk değer istemci tarafından taklit edilebilir).
    options.RealIpHeader = Environment.GetEnvironmentVariable("CLIENT_IP_HEADER") ?? "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule { Endpoint = "POST:/api/v1/auth/login", Period = "15m", Limit = 5 },
        new RateLimitRule { Endpoint = "POST:/api/v1/contact", Period = "10m", Limit = 3 },
        new RateLimitRule { Endpoint = "POST:/api/leaderboard", Period = "1m", Limit = 10 }, // Leaderboard için rate limit
        new RateLimitRule { Endpoint = "POST:/api/leaderboard/session", Period = "1m", Limit = 10 }, // Oyun oturumu açma
        new RateLimitRule { Endpoint = "GET:/api/leaderboard", Period = "1m", Limit = 30 },  // GET için daha yüksek limit
        new RateLimitRule { Endpoint = "*", Period = "1m", Limit = 60 }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS - Angular için düzenlendi
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        var allowedOrigins = new List<string> { 
            "http://localhost:4200",
            "http://localhost:3000",
            "https://localhost:4200"
        };
        
        // Production domain varsa ekle
        var productionDomain = Environment.GetEnvironmentVariable("ALLOWED_ORIGIN");
        if (!string.IsNullOrEmpty(productionDomain))
            allowedOrigins.Add(productionDomain);
        
        // Frontend domain'iniz (eğer varsa)
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        if (!string.IsNullOrEmpty(frontendUrl))
            allowedOrigins.Add(frontendUrl);

        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Credentials için
    });
});

// PostgreSQL — .env'den oku
var dbConnection = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("DB_CONNECTION bulunamadı!");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        dbConnection,
        o =>
        {
            o.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        }));

// JWT Secret — .env / ortam değişkeninden oku (production'da appsettings yedeği kullanılmaz)
var jwtSecret = JwtSecretResolver.Resolve(builder.Configuration, builder.Environment);

// JWT Secret'ı config'e yaz
builder.Configuration["JwtSettings:SecretKey"] = jwtSecret;

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<GameSessionStore>();

// JWT doğrulama
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<EmailService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Portfolio API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Token giriniz."
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Middleware sıralaması önemli!
app.UseIpRateLimiting();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("AllowAngular");

// Swagger sadece development'ta açık
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS yönlendirme - production'da açın
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseResponseCaching();

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseAuthorization();
app.MapControllers();

// Seed - tek sefer çalışsın
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

app.Run();

public partial class Program { } // Entegrasyon testleri için
