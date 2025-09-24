using Core.Interfaces;

namespace API.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Skip API key validation for certain paths
        if (ShouldSkipApiKeyValidation(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Check if API key is required for this endpoint
        if (!IsApiKeyRequired(context))
        {
            await _next(context);
            return;
        }

        var apiKey = ExtractApiKey(context.Request);
        
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("API key missing for request: {Method} {Path} from {RemoteIpAddress}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 401;
            context.Response.Headers["WWW-Authenticate"] = "ApiKey";
            await context.Response.WriteAsync("API key is required");
            return;
        }

        var isValid = await apiKeyService.ValidateApiKeyAsync(apiKey);
        
        if (!isValid)
        {
            _logger.LogWarning("Invalid API key for request: {Method} {Path} from {RemoteIpAddress}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 401;
            context.Response.Headers["WWW-Authenticate"] = "ApiKey";
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        // Add API key info to context for logging
        context.Items["ApiKey"] = apiKey;
        
        // Update last used
        await apiKeyService.UpdateLastUsedAsync(apiKey);

        await _next(context);
    }

    private static bool ShouldSkipApiKeyValidation(PathString path)
    {
        var skipPaths = new[]
        {
            "/swagger",
            "/health",
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/refresh-token"
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsApiKeyRequired(HttpContext context)
    {
        // Check if the endpoint requires API key
        var endpoint = context.GetEndpoint();
        
        if (endpoint?.Metadata?.GetMetadata<RequireApiKeyAttribute>() != null)
        {
            return true;
        }

        // Check if it's an API endpoint (starts with /api/)
        return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractApiKey(HttpRequest request)
    {
        // Try to get API key from header
        if (request.Headers.TryGetValue("X-API-Key", out var headerValue))
        {
            return headerValue.FirstOrDefault();
        }

        // Try to get API key from Authorization header
        if (request.Headers.TryGetValue("Authorization", out var authValue))
        {
            var authHeader = authValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring(7);
            }
        }

        // Try to get API key from query string (for testing purposes)
        if (request.Query.TryGetValue("api_key", out var queryValue))
        {
            return queryValue.FirstOrDefault();
        }

        return null;
    }
}

// Attribute to mark endpoints that require API key
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireApiKeyAttribute : Attribute
{
}
