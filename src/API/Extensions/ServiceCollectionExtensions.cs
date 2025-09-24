using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using API.Middleware;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using API.Filters;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers
        services.AddControllers(options =>
        {
            options.Filters.Add<AuditActionFilter>();
        });
        services.AddScoped<AuditActionFilter>();
        
        // Add API Explorer for Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "MarketPlace API",
                Version = "v1",
                Description = "Multi-vendor marketplace API"
            });

            // JWT Bearer auth for Swagger
            var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter 'Bearer {token}'",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            c.AddSecurityDefinition("Bearer", securityScheme);
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    securityScheme,
                    Array.Empty<string>()
                }
            });

            // Guidance for JWT usage
            c.DocumentFilter<SwaggerJwtNotesFilter>();

            // ApiResponse samples for success and errors
            c.OperationFilter<API.Swagger.ApiResponseOperationFilter>();
        });

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                var origins = (configuration["Cors:AllowedOrigins"] ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
                else
                {
                    policy.DisallowCredentials().WithOrigins();
                }
            });
        });

        // Add Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found"))),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Add Authorization
        services.AddAuthorization();

        // Add FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<Program>();

        // HttpClient factory (for external health checks)
        services.AddHttpClient();

        // Add Health Checks (DB, Redis, External Payment) via custom checks
        services.AddHealthChecks()
            .AddCheck<API.HealthChecks.SqlHealthCheck>("sql-db")
            .AddCheck<API.HealthChecks.RedisHealthCheck>("redis")
            .AddCheck<API.HealthChecks.ExternalUrlHealthCheck>("payment-api");

        return services;
    }

    private class SwaggerJwtNotesFilter : Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter
    {
        public void Apply(Microsoft.OpenApi.Models.OpenApiDocument swaggerDoc, Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext context)
        {
            swaggerDoc.Info.Description += "\n\nAuthorization: Bearer {access_token}. Refresh token HttpOnly cookie ile taşınır (refresh_token).";
        }
    }

    public static IServiceCollection AddSerilogServices(this IServiceCollection services, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "MarketPlace.API")
            .WriteTo.Console()
            .WriteTo.File("logs/marketplace-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddSerilog();

        return services;
    }
}
