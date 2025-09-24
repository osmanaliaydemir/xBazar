using Application.Services;
using Core.Interfaces;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services;

public class SecurityEventService : ISecurityEventService
{
    private readonly IUnitOfWork _uow;

    public SecurityEventService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task LogAsync(string eventType, string? subject, string? ip, string? userAgent, object? details = null)
    {
        var ev = new Core.Entities.SecurityEvent
        {
            EventType = eventType,
            Subject = subject,
            IpAddress = ip,
            UserAgent = userAgent,
            Details = details == null ? null : JsonSerializer.Serialize(details),
            OccurredAt = DateTime.UtcNow
        };
        await _uow.SecurityEvents.AddAsync(ev);
        await _uow.SaveChangesAsync();
    }

    public async Task<List<Core.Entities.SecurityEvent>> GetAsync(DateTime from, DateTime to, string? eventType = null)
    {
        var all = await _uow.SecurityEvents.GetAllAsync(e => e.OccurredAt >= from && e.OccurredAt <= to && !e.IsDeleted);
        if (!string.IsNullOrWhiteSpace(eventType))
        {
            all = all.Where(e => e.EventType == eventType).ToList();
        }
        return all.OrderByDescending(e => e.OccurredAt).ToList();
    }

    public async Task<byte[]> ExportCsvAsync(DateTime from, DateTime to, string? eventType = null)
    {
        var events = await GetAsync(from, to, eventType);
        var sb = new StringBuilder();
        sb.AppendLine("OccurredAt,EventType,Subject,IpAddress,UserAgent,Details");
        foreach (var e in events.OrderBy(e => e.OccurredAt))
        {
            sb.AppendLine($"{e.OccurredAt:O},{Escape(e.EventType)},{Escape(e.Subject)},{Escape(e.IpAddress)},{Escape(e.UserAgent)},{Escape(e.Details)}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
