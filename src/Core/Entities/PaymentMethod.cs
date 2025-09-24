using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class PaymentMethod : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Provider { get; set; } = "iyzico"; // payment provider name

    [Required]
    [MaxLength(200)]
    public string Token { get; set; } = string.Empty; // provider token, not PAN

    [MaxLength(4)]
    public string? Last4 { get; set; }

    [MaxLength(50)]
    public string? Brand { get; set; }

    [MaxLength(2)]
    public string? ExpiryMonth { get; set; }

    [MaxLength(4)]
    public string? ExpiryYear { get; set; }

    [MaxLength(100)]
    public string? Label { get; set; } // user friendly name

    public bool IsDefault { get; set; } = false;

    // Navigation
    public virtual User User { get; set; } = null!;
}
