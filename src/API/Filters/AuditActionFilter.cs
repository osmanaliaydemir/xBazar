using Core.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text.Json;

namespace API.Filters;

public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _uow;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = false };

    public AuditActionFilter(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        try
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var table = descriptor?.ControllerName ?? "Unknown";
            var actionName = descriptor?.ActionName ?? "Unknown";
            var http = context.HttpContext;

            var userId = http.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = http.User?.FindFirstValue(ClaimTypes.Email);
            var ip = http.Connection.RemoteIpAddress?.ToString();
            var ua = http.Request.Headers["User-Agent"].ToString();

            // Sanitize and serialize action arguments (exclude files/streams)
            var args = context.ActionArguments
                .Where(kv => kv.Value is not Stream && kv.Value is not IFormFile && kv.Value is not IFormFileCollection)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var newValues = args.Count > 0 ? JsonSerializer.Serialize(args, JsonOptions) : null;

            var audit = new Core.Entities.AuditLog
            {
                TableName = table,
                RecordId = Guid.Empty,
                Action = $"{http.Request.Method} {actionName}",
                UserId = userId,
                UserEmail = userEmail,
                IpAddress = ip,
                UserAgent = ua,
                OldValues = null,
                NewValues = newValues,
                Timestamp = DateTime.UtcNow
            };

            await _uow.AuditLogs.AddAsync(audit);
            await _uow.SaveChangesAsync();
        }
        catch
        {
            // swallow audit errors to not affect main pipeline
        }
    }
}
