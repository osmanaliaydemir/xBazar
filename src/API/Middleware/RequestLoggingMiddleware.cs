using System.Diagnostics;
using System.Text;

namespace API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.TraceIdentifier;
        
        // Add correlation ID to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Log request
        _logger.LogInformation(
            "Request started: {Method} {Path} from {RemoteIpAddress} with CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress,
            correlationId);

        // Capture request body for logging (if needed)
        if (ShouldLogRequestBody(context))
        {
            context.Request.EnableBuffering();
            var requestBody = await ReadRequestBodyAsync(context.Request);
            _logger.LogDebug("Request body: {RequestBody}", requestBody);
        }

        // Call next middleware
        await _next(context);

        stopwatch.Stop();

        // Log response
        _logger.LogInformation(
            "Request completed: {Method} {Path} with status {StatusCode} in {ElapsedMilliseconds}ms with CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }

    private static bool ShouldLogRequestBody(HttpContext context)
    {
        // Only log request body for POST, PUT, PATCH requests
        var method = context.Request.Method.ToUpper();
        return method is "POST" or "PUT" or "PATCH";
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
        catch
        {
            return "Unable to read request body";
        }
    }
}
