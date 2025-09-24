using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Review : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public int Rating { get; set; } // 1-5
    
    [MaxLength(2000)]
    public string? Comment { get; set; }
    
    public bool IsVerifiedPurchase { get; set; } = true;
    
    public bool IsApproved { get; set; } = true;
    
    public bool IsHelpful { get; set; } = false;
    
    public int HelpfulCount { get; set; } = 0;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
}
