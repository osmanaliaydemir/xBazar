using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Payment : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PaymentMethod { get; set; } = string.Empty; // "iyzico", "paytr", etc.
    
    [Required]
    [MaxLength(100)]
    public string PaymentProviderTransactionId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    [MaxLength(50)]
    public string? Currency { get; set; } = "TRY";
    
    [MaxLength(1000)]
    public string? ProviderResponse { get; set; } // JSON response from payment provider
    
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime? FailedAt { get; set; }
    
    [MaxLength(100)]
    public string? RefundTransactionId { get; set; }
    
    public DateTime? RefundedAt { get; set; }
    
    public decimal? RefundAmount { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5,
    PartiallyRefunded = 6
}
