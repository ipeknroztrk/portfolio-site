using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioSite.Application.Data;
using PortfolioSite.Application.DTOs;
using PortfolioSite.Application.Interfaces;
using PortfolioSite.Application.Services;
using PortfolioSite.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// CORS — Angular'ın API'ye erişmesine izin ver
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// PostgreSQL bağlantısı — connection string appsettings.json'dan okunur
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT ayarlarını appsettings.json'daki "JwtSettings" bölümünden oku
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Bağımlılık enjeksiyonu — interface'e karşılık gelen servis sınıflarını kaydet
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT doğrulama ayarları
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           // Token'ın kim tarafından üretildiğini doğrula
        ValidateAudience = true,         // Token'ın kime hitap ettiğini doğrula
        ValidateLifetime = true,         // Token süresi dolmuşsa reddet
        ValidateIssuerSigningKey = true, // İmzayı doğrula
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero        // Varsayılan 5 dakika toleransı kaldır — token süresi tam dakikada dolar
    };
});

builder.Services.AddAuthorization();

// Swagger — sadece geliştirme ortamında
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Portfolio API", Version = "v1" });

    // Swagger'a JWT token girme kutusu ekle
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

// Tüm beklenmeyen hataları yakalar — stack trace dışarıya sızmaz
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowAngular");

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseAuthentication(); // Önce kimlik doğrula
app.UseAuthorization();  // Sonra yetki kontrol et — sıra önemli!
app.MapControllers();

// Uygulama ilk başladığında admin yoksa otomatik oluştur
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

app.Run();