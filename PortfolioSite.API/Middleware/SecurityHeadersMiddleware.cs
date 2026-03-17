namespace PortfolioSite.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Clickjacking koruması
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        
        // MIME type sniffing koruması
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // XSS koruması
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        
        // Referrer bilgisini kısıtla
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Server bilgisini gizle
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        await _next(context);
    }
}