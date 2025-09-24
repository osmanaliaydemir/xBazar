using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class SecurityEvent : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty; // login_failed, rate_limit, policy_denied

    [MaxLength(200)]
    public string? Subject { get; set; } // user/email/ip

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(1000)]
    public string? UserAgent { get; set; }

    public string? Details { get; set; } // JSON

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
