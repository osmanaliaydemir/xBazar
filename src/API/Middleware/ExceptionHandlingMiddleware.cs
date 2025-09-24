using System.Net;
using System.Text.Json;
using Application.DTOs.Common;
using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = "An error occurred while processing your request",
            Detail = exception.Message,
            Instance = context.Request.Path,
            Status = (int)HttpStatusCode.InternalServerError
        };

        switch (exception)
        {
            case ArgumentException argEx:
                problem.Title = "Bad Request";
                problem.Detail = argEx.Message;
                problem.Status = (int)HttpStatusCode.BadRequest;
                break;
            case UnauthorizedAccessException:
                problem.Title = "Unauthorized";
                problem.Detail = "Unauthorized access";
                problem.Status = (int)HttpStatusCode.Unauthorized;
                break;
            case KeyNotFoundException:
                problem.Title = "Not Found";
                problem.Detail = "Resource not found";
                problem.Status = (int)HttpStatusCode.NotFound;
                break;
            case InvalidOperationException invalidOpEx:
                problem.Title = "Invalid Operation";
                problem.Detail = invalidOpEx.Message;
                problem.Status = (int)HttpStatusCode.BadRequest;
                break;
            case DomainException domainEx:
                problem.Title = domainEx.ErrorCode ?? "Domain Error";
                problem.Detail = domainEx.Message;
                problem.Status = (int)domainEx.StatusCode;
                break;
            case NotFoundException nf:
                problem.Title = nf.ErrorCode ?? "Not Found";
                problem.Detail = nf.Message;
                problem.Status = (int)nf.StatusCode;
                break;
            case ConflictException cf:
                problem.Title = cf.ErrorCode ?? "Conflict";
                problem.Detail = cf.Message;
                problem.Status = (int)cf.StatusCode;
                break;
            case ValidationException ve:
                problem.Title = ve.ErrorCode ?? "Validation Error";
                problem.Detail = ve.Message;
                problem.Status = (int)ve.StatusCode;
                problem.Extensions["errors"] = ve.Errors;
                break;
            case ForbiddenException fb:
                problem.Title = fb.ErrorCode ?? "Forbidden";
                problem.Detail = fb.Message;
                problem.Status = (int)fb.StatusCode;
                break;
            default:
                problem.Title = "Internal Server Error";
                problem.Detail = "An unexpected error occurred";
                problem.Status = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
        var jsonResponse = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(jsonResponse);
    }
}
