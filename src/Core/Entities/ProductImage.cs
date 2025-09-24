using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class ProductImage : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? AltText { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsMain { get; set; } = false;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
