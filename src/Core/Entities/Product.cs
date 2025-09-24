using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Product : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;
    
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public decimal? CompareAtPrice { get; set; }
    
    public int StockQuantity { get; set; } = 0;
    
    public int ReservedQuantity { get; set; } = 0;
    
    public bool TrackQuantity { get; set; } = true;
    
    public bool AllowBackorder { get; set; } = false;
    
    [MaxLength(500)]
    public string? MainImageUrl { get; set; }
    
    public decimal Weight { get; set; } = 0;
    
    [MaxLength(50)]
    public string? WeightUnit { get; set; } = "kg";
    
    public decimal Length { get; set; } = 0;
    
    public decimal Width { get; set; } = 0;
    
    public decimal Height { get; set; } = 0;
    
    [MaxLength(50)]
    public string? DimensionUnit { get; set; } = "cm";
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFeatured { get; set; } = false;
    
    public int ViewCount { get; set; } = 0;
    
    public decimal AverageRating { get; set; } = 0;
    
    public int ReviewCount { get; set; } = 0;
    
    // Navigation properties
    public virtual Store Store { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
