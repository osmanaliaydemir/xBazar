using Core.Entities;

namespace Core.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<RefundResult> ProcessRefundAsync(RefundRequest request);
    Task<PaymentStatus> GetPaymentStatusAsync(string transactionId);
    Task<bool> ValidatePaymentAsync(string transactionId, decimal expectedAmount);
}

public class PaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string? Installment { get; set; }
    public string? CallbackUrl { get; set; }
    public string? IdempotencyKey { get; set; }
}

public class RefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? RedirectUrl { get; set; }
    public PaymentStatus Status { get; set; }
}

public class RefundResult
{
    public bool IsSuccess { get; set; }
    public string RefundTransactionId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
