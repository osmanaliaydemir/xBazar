using API.Middleware;

namespace API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        // Configure the HTTP request pipeline
        
        // Security headers middleware (should be first)
        app.UseMiddleware<SecurityHeadersMiddleware>();
        
        // Request logging middleware
        app.UseMiddleware<RequestLoggingMiddleware>();
        
        // Exception handling middleware
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        
        // Rate limiting middleware
        app.UseMiddleware<RateLimitingMiddleware>();
        
        // API Key validation middleware
        app.UseMiddleware<ApiKeyMiddleware>();

        // Swagger/OpenAPI
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Static files
        app.UseStaticFiles();

        // CORS
        app.UseCors("DefaultCors");

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Map controllers
        app.MapControllers();

        // Health checks
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/ready");

        return app;
    }
}
