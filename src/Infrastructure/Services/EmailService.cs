using Core.Interfaces;
using Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        var smtpHost = configuration["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        var smtpUsername = configuration["Email:Username"] ?? "";
        var smtpPassword = configuration["Email:Password"] ?? "";
        
        _fromEmail = configuration["Email:FromEmail"] ?? "noreply@marketplace.com";
        _fromName = configuration["Email:FromName"] ?? "Marketplace";
        
        _smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            
            message.To.Add(to);
            
            await _smtpClient.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmationLink)
    {
        var subject = "E-posta Adresinizi Doğrulayın";
        var body = await LoadTemplateAsync("EmailConfirmation.html");
        body = body.Replace("{{action_url}}", confirmationLink);
        
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendPasswordResetAsync(string to, string resetLink)
    {
        var subject = "Şifre Sıfırlama";
        var body = await LoadTemplateAsync("PasswordReset.html");
        body = body.Replace("{{action_url}}", resetLink);
        
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendOrderConfirmationAsync(string to, string orderNumber, decimal totalAmount)
    {
        var subject = $"Sipariş Onayı - {orderNumber}";
        var body = $@"
            <h2>Siparişiniz Alındı!</h2>
            <p>Merhaba,</p>
            <p>Siparişiniz başarıyla alındı. Sipariş numaranız: <strong>{orderNumber}</strong></p>
            <p>Toplam tutar: <strong>{totalAmount:C}</strong></p>
            <p>Siparişinizin durumunu takip etmek için hesabınıza giriş yapabilirsiniz.</p>
            <p>Teşekkürler!</p>
        ";
        
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendOrderStatusUpdateAsync(string to, string orderNumber, string status)
    {
        var subject = $"Sipariş Durumu Güncellendi - {orderNumber}";
        var body = $@"
            <h2>Sipariş Durumu Güncellendi</h2>
            <p>Merhaba,</p>
            <p>Siparişinizin durumu güncellendi: <strong>{status}</strong></p>
            <p>Sipariş numarası: <strong>{orderNumber}</strong></p>
            <p>Detaylı bilgi için hesabınıza giriş yapabilirsiniz.</p>
        ";
        
        await SendEmailAsync(to, subject, body);
    }

    public async Task SendStoreApprovalAsync(string to, string storeName, bool isApproved, string? reason = null)
    {
        var subject = isApproved ? "Mağaza Başvurunuz Onaylandı" : "Mağaza Başvurunuz Reddedildi";
        var body = isApproved 
            ? $@"
                <h2>Mağaza Başvurunuz Onaylandı!</h2>
                <p>Merhaba,</p>
                <p><strong>{storeName}</strong> mağaza başvurunuz onaylandı!</p>
                <p>Artık ürünlerinizi yüklemeye başlayabilirsiniz.</p>
                <p>Mağaza paneline giriş yapmak için hesabınıza giriş yapın.</p>
            "
            : $@"
                <h2>Mağaza Başvurunuz Reddedildi</h2>
                <p>Merhaba,</p>
                <p><strong>{storeName}</strong> mağaza başvurunuz reddedildi.</p>
                <p>Sebep: {reason ?? "Belirtilmemiş"}</p>
                <p>Yeni bir başvuru yapmak için lütfen gerekli düzeltmeleri yapın.</p>
            ";
        
        await SendEmailAsync(to, subject, body);
    }

    public void Dispose()
    {
        _smtpClient?.Dispose();
    }

    private static async Task<string> LoadTemplateAsync(string templateName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "EmailTemplates", templateName);
        if (!File.Exists(path))
        {
            return "<p>{{action_url}}</p>";
        }
        return await File.ReadAllTextAsync(path);
    }
}
