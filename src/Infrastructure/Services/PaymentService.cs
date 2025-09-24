using Core.Interfaces;
using Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly HttpClient _httpClient;
    private readonly PaymentOptions _options;

    public PaymentService(ILogger<PaymentService> logger, HttpClient httpClient, IOptions<PaymentOptions> options)
    {
        _logger = logger;
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId}, amount {Amount}, idem {Idemp}", request.OrderId, request.Amount, request.IdempotencyKey);

            // Mock 3DS: return redirect url when amount > 0
            var needsRedirect = request.Amount > 0;
            await Task.Delay(300);
            if (needsRedirect)
            {
                return new PaymentResult
                {
                    IsSuccess = true,
                    TransactionId = Guid.NewGuid().ToString(),
                    RedirectUrl = request.CallbackUrl != null ? request.CallbackUrl + "?status=success" : null,
                    Status = Core.Entities.PaymentStatus.Processing
                };
            }

            return new PaymentResult
            {
                IsSuccess = true,
                TransactionId = Guid.NewGuid().ToString(),
                Status = Core.Entities.PaymentStatus.Completed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment for order {OrderId}", request.OrderId);
            return new PaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "Ödeme sırasında bir hata oluştu.",
                Status = Core.Entities.PaymentStatus.Failed
            };
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        try
        {
            _logger.LogInformation("Processing refund for transaction {TransactionId}", request.TransactionId);
            await Task.Delay(300);
            return new RefundResult
            {
                IsSuccess = true,
                RefundTransactionId = Guid.NewGuid().ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process refund for transaction {TransactionId}", request.TransactionId);
            return new RefundResult
            {
                IsSuccess = false,
                ErrorMessage = "İade işlemi sırasında bir hata oluştu."
            };
        }
    }

    public async Task<Core.Entities.PaymentStatus> GetPaymentStatusAsync(string transactionId)
    {
        await Task.Delay(200);
        return Core.Entities.PaymentStatus.Completed;
    }

    public async Task<bool> ValidatePaymentAsync(string transactionId, decimal expectedAmount)
    {
        await Task.Delay(100);
        return true;
    }
}

public class PaymentOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string CallbackUrl { get; set; } = "https://localhost:7001/api/payments/callback";
}
