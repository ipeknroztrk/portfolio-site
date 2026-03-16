using System.Net;
using System.Text.Json;

namespace PortfolioSite.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next; // Sonraki middleware'e geçiş için
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // İsteği bir sonraki middleware'e ilet
            await _next(context);
        }
        catch (Exception ex)
        {
            // Hata logla — stack trace sadece sunucuda kalır, dışarıya sızmaz
            _logger.LogError(ex, "Beklenmeyen hata: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        // Tüm beklenmeyen hatalar 500 döner — hata detayı kullanıcıya gösterilmez
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = "Sunucu hatasi olustu.",
            // detail şimdilik boş — geliştirme ortamında doldurmak istersen buraya ex.Message yazabilirsin
            detail = string.Empty
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}