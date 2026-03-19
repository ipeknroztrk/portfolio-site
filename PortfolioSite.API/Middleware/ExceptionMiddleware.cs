using System.Net;
using System.Text.Json;

namespace PortfolioSite.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = "Sunucu hatasi olustu.",
            // Production'da detail gizle
            detail = _env.IsDevelopment()
                ? ex.Message + " | " + ex.InnerException?.Message
                : ""
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}