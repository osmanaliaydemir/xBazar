namespace Application.Services;

public interface ISecurityEventService
{
    Task LogAsync(string eventType, string? subject, string? ip, string? userAgent, object? details = null);
    Task<List<Core.Entities.SecurityEvent>> GetAsync(DateTime from, DateTime to, string? eventType = null);
    Task<byte[]> ExportCsvAsync(DateTime from, DateTime to, string? eventType = null);
}
