using System.Collections.Concurrent;
using System.Net;

namespace API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _requests = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly HashSet<string> _whitelistPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/ready",
        "/swagger",
        "/swagger/index.html"
    };

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _maxRequests = configuration.GetValue<int>("RateLimiting:MaxRequests", 100);
        _timeWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimiting:TimeWindowMinutes", 1));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for whitelisted endpoints
        if (IsWhitelisted(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var rateLimitInfo = _requests.AddOrUpdate(
            clientId,
            new RateLimitInfo { Count = 1, FirstRequest = now },
            (key, existing) =>
            {
                if (now - existing.FirstRequest > _timeWindow)
                {
                    return new RateLimitInfo { Count = 1, FirstRequest = now };
                }
                return new RateLimitInfo { Count = existing.Count + 1, FirstRequest = existing.FirstRequest };
            });

        if (rateLimitInfo.Count > _maxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}. Count: {Count}", clientId, rateLimitInfo.Count);
            
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = Math.Ceiling(_timeWindow.TotalSeconds).ToString();
            context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.Response.Headers["X-RateLimit-Reset"] = (rateLimitInfo.FirstRequest.Add(_timeWindow) - now).TotalSeconds.ToString();
            
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, _maxRequests - rateLimitInfo.Count).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = Math.Max(0, (int)(rateLimitInfo.FirstRequest.Add(_timeWindow) - now).TotalSeconds).ToString();

        await _next(context);
    }

    private bool IsWhitelisted(PathString path)
    {
        var p = path.ToString();
        if (string.IsNullOrEmpty(p)) return false;
        // allow swagger and health endpoints
        if (p.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)) return true;
        if (p.Equals("/health", StringComparison.OrdinalIgnoreCase)) return true;
        if (p.Equals("/ready", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID if authenticated
        var userId = context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime FirstRequest { get; set; }
    }
}
