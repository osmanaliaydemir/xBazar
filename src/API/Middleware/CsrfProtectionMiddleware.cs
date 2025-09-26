using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace API.Middleware;

public class CsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CsrfProtectionMiddleware> _logger;

    public CsrfProtectionMiddleware(RequestDelegate next, ILogger<CsrfProtectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip CSRF protection for safe methods and certain endpoints
        if (ShouldSkipCsrfProtection(context))
        {
            await _next(context);
            return;
        }

        // Generate CSRF token if not exists
        if (!context.Request.Cookies.ContainsKey("XSRF-TOKEN"))
        {
            var csrfToken = GenerateCsrfToken();
            context.Response.Cookies.Append("XSRF-TOKEN", csrfToken, new CookieOptions
            {
                HttpOnly = false, // Must be accessible to JavaScript
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });
        }

        // Validate CSRF token for state-changing operations
        if (RequiresCsrfValidation(context))
        {
            var headerToken = context.Request.Headers["X-XSRF-TOKEN"].FirstOrDefault();
            var cookieToken = context.Request.Cookies["XSRF-TOKEN"];

            if (string.IsNullOrEmpty(headerToken) || string.IsNullOrEmpty(cookieToken) || 
                !string.Equals(headerToken, cookieToken, StringComparison.Ordinal))
            {
                _logger.LogWarning("CSRF token validation failed for {Method} {Path} from {RemoteIpAddress}",
                    context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("CSRF token validation failed");
                return;
            }
        }

        await _next(context);
    }

    private static bool ShouldSkipCsrfProtection(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        var method = context.Request.Method.ToUpperInvariant();

        // Skip for safe methods
        if (method == "GET" || method == "HEAD" || method == "OPTIONS")
        {
            return true;
        }

        // Skip for certain endpoints
        var skipPaths = new[]
        {
            "/swagger",
            "/health",
            "/ready",
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/refresh-token"
        };

        return skipPaths.Any(skipPath => path?.StartsWith(skipPath, StringComparison.OrdinalIgnoreCase) == true);
    }

    private static bool RequiresCsrfValidation(HttpContext context)
    {
        var method = context.Request.Method.ToUpperInvariant();
        return method == "POST" || method == "PUT" || method == "PATCH" || method == "DELETE";
    }

    private static string GenerateCsrfToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
