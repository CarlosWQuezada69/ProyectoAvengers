using System.Security.Cryptography;

namespace ProyectoAvengers.Api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    private static readonly string[] SwaggerPaths = ["/swagger", "/swagger/"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.TrimEnd('/') ?? "";

        if (SwaggerPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        context.Items["CspNonce"] = nonce;

        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "0";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        var apiOrigin = _env.IsDevelopment()
            ? "http://localhost:4200"
            : "";

        context.Response.Headers["Content-Security-Policy"] =
            $"default-src 'self'; " +
            $"script-src 'self' 'nonce-{nonce}'; " +
            $"style-src 'self' 'nonce-{nonce}'; " +
            $"img-src 'self' data:; " +
            $"font-src 'self'; " +
            $"connect-src 'self' {apiOrigin}; " +
            $"frame-ancestors 'none';";

        await _next(context);
    }
}
