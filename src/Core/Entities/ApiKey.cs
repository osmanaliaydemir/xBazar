using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class ApiKey : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(64)]
    public string Key { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? UserId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? ExpiresAt { get; set; }
    
    public DateTime? LastUsedAt { get; set; }
    
    public int UsageCount { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Environment { get; set; } // Development, Staging, Production
    
    // Navigation properties
    public virtual User? User { get; set; }
}
