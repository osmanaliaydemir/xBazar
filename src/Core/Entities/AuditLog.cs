using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class AuditLog : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    [Required]
    public Guid RecordId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    
    [MaxLength(100)]
    public string? UserId { get; set; }
    
    [MaxLength(100)]
    public string? UserEmail { get; set; }
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [MaxLength(1000)]
    public string? UserAgent { get; set; }
    
    public string? OldValues { get; set; } // JSON string
    public string? NewValues { get; set; } // JSON string
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
