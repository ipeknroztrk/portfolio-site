using System.Net;
using System.Text.Json;

namespace PortfolioSite.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

   private Task HandleExceptionAsync(HttpContext context, Exception ex)
{
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

    // Hata detayları sadece development ortamında istemciye gönderilir
    object response = _env.IsDevelopment()
        ? new
        {
            statusCode = context.Response.StatusCode,
            message = ex.Message,
            detail = ex.InnerException?.Message,
            stackTrace = ex.StackTrace
        }
        : new
        {
            statusCode = context.Response.StatusCode,
            message = "An unexpected error occurred"
        };

    return context.Response.WriteAsync(JsonSerializer.Serialize(response));
}
}