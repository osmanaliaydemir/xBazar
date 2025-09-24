using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class CouponUsage : BaseEntity
{
    [Required]
    public Guid CouponId { get; set; }
    
    [Required]
    public Guid OrderId { get; set; }
    
    public Guid? UserId { get; set; } // Null for guest users
    
    [Required]
    public decimal DiscountAmount { get; set; }
    
    // Navigation properties
    public virtual Coupon Coupon { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual User? User { get; set; }
}
