using API.Extensions;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Core.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Application.Validators;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configuration providers (User Secrets in Development)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}
builder.Configuration.AddEnvironmentVariables();

// Add Serilog
builder.Services.AddSerilogServices(builder.Configuration);

// Kestrel and request limits
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

// Forwarded headers for reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add API services
builder.Services.AddApiServices(builder.Configuration);

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    // Policies will be configured after container is built, using a post-build action
});

var app = builder.Build();

// Use forwarded headers (before other middleware)
app.UseForwardedHeaders();

// Use API middleware
app.UseApiMiddleware();

// Configure authorization policies using the built service provider (safe at this point)
using (var scope = app.Services.CreateScope())
{
    // Authorization policies are now configured in Infrastructure.Authorization.AuthorizationExtensions
}

app.Run();