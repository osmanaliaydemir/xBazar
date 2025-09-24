using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class OrderStatusHistory : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime StatusDate { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}
