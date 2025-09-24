using Application.Services;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _uow;

    public NotificationService(ILogger<NotificationService> logger, IEmailService emailService, IUnitOfWork uow)
    {
        _logger = logger;
        _emailService = emailService;
        _uow = uow;
    }

    public async Task SendInAppAsync(Guid userId, string title, string message, string? link = null)
    {
        var sysThread = await GetOrCreateSystemThreadAsync(userId);
        var msg = new Message
        {
            ThreadId = sysThread.Id,
            SenderId = Guid.Empty,
            ReceiverId = userId,
            Type = MessageType.System,
            Content = string.IsNullOrEmpty(link) ? $"{title}\n{message}" : $"{title}\n{message}\n{link}",
            CreatedAt = DateTime.UtcNow
        };
        await _uow.Messages.AddAsync(msg);
        sysThread.LastMessageAt = DateTime.UtcNow;
        await _uow.MessageThreads.UpdateAsync(sysThread);
        await _uow.SaveChangesAsync();
        _logger.LogInformation("In-app notification sent to {UserId}: {Title}", userId, title);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlBody)
    {
        await _emailService.SendEmailAsync(email, subject, htmlBody);
    }

    public async Task NotifyOrderCreatedAsync(Guid userId, string orderNumber, decimal total)
    {
        await SendInAppAsync(userId, "Sipariş Oluşturuldu", $"Sipariş numaranız: {orderNumber}, Toplam: {total:C}");
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user?.Email != null)
        {
            var html = await LoadTemplateAsync("OrderCreated.html");
            html = html.Replace("{{order_number}}", orderNumber)
                       .Replace("{{total}}", total.ToString("C"))
                       .Replace("{{order_link}}", $"https://localhost:7001/orders/{orderNumber}");
            await _emailService.SendEmailAsync(user.Email, "Sipariş Oluşturuldu", html);
        }
    }

    public async Task NotifyOrderStatusChangedAsync(Guid userId, string orderNumber, string status)
    {
        await SendInAppAsync(userId, "Sipariş Durumu Güncellendi", $"{orderNumber} → {status}");
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user?.Email != null)
        {
            var html = await LoadTemplateAsync("OrderStatusChanged.html");
            html = html.Replace("{{order_number}}", orderNumber)
                       .Replace("{{status}}", status)
                       .Replace("{{order_link}}", $"https://localhost:7001/orders/{orderNumber}");
            await _emailService.SendEmailAsync(user.Email, "Sipariş Durumu Güncellendi", html);
        }
    }

    private static async Task<string> LoadTemplateAsync(string templateName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", templateName);
        if (!File.Exists(path))
        {
            return "<p></p>";
        }
        return await File.ReadAllTextAsync(path);
    }

    private async Task<MessageThread> GetOrCreateSystemThreadAsync(Guid userId)
    {
        var thread = await _uow.MessageThreads.GetAsync(t => (t.User1Id == Guid.Empty && t.User2Id == userId) || (t.User1Id == userId && t.User2Id == Guid.Empty));
        if (thread != null) return thread;
        var newThread = new MessageThread
        {
            Subject = "System Notifications",
            User1Id = Guid.Empty,
            User2Id = userId,
            IsActive = true,
            LastMessageAt = DateTime.UtcNow
        };
        await _uow.MessageThreads.AddAsync(newThread);
        await _uow.SaveChangesAsync();
        return newThread;
    }
}

