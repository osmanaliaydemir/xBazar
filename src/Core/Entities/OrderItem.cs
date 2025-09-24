using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class OrderItem : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ProductSKU { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? ProductImageUrl { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal UnitPrice { get; set; }
    
    [Required]
    public decimal TotalPrice { get; set; }
    
    public decimal? DiscountAmount { get; set; }
    
    [MaxLength(1000)]
    public string? ProductAttributes { get; set; } // JSON string for product variants
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
