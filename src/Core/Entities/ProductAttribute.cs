using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class ProductAttribute : BaseEntity
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;
    
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
