namespace Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailConfirmationAsync(string to, string confirmationLink);
    Task SendPasswordResetAsync(string to, string resetLink);
    Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount);
    Task SendOrderStatusUpdateAsync(string to, string orderNumber, string status);
    Task SendStoreApprovalAsync(string to, string storeName, bool isApproved, string? reason = null);
}
