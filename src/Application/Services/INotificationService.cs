namespace Application.Services;

public interface INotificationService
{
    Task SendInAppAsync(Guid userId, string title, string message, string? link = null);
    Task SendEmailAsync(string email, string subject, string htmlBody);
    Task NotifyOrderCreatedAsync(Guid userId, string orderNumber, decimal total);
    Task NotifyOrderStatusChangedAsync(Guid userId, string orderNumber, string status);
}

