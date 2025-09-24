using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Coupon : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public CouponType Type { get; set; }
    
    [Required]
    public decimal Value { get; set; }
    
    public decimal? MinimumOrderAmount { get; set; }
    
    public decimal? MaximumDiscountAmount { get; set; }
    
    public int? UsageLimit { get; set; }
    
    public int UsedCount { get; set; } = 0;
    
    public int? UsageLimitPerUser { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidUntil { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Guid? StoreId { get; set; } // Null for global coupons
    
    public Guid? CategoryId { get; set; } // Null for all categories
    
    public Guid? ProductId { get; set; } // Null for all products
    
    public bool IsFirstOrderOnly { get; set; } = false;
    
    public bool IsFreeShipping { get; set; } = false;
    
    // Navigation properties
    public virtual Store? Store { get; set; }
    public virtual Category? Category { get; set; }
    public virtual Product? Product { get; set; }
    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
}

public enum CouponType
{
    Percentage = 0,
    FixedAmount = 1,
    FreeShipping = 2
}
