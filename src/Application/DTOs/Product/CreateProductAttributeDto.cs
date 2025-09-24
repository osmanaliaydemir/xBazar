using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product;

public class CreateProductAttributeDto
{
    [Required(ErrorMessage = "Attribute name is required")]
    [MinLength(2, ErrorMessage = "Attribute name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Attribute name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Attribute value is required")]
    [MinLength(1, ErrorMessage = "Attribute value must be at least 1 character")]
    [MaxLength(500, ErrorMessage = "Attribute value cannot exceed 500 characters")]
    public string Value { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
    public string? Unit { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sort order must be greater than or equal to 0")]
    public int SortOrder { get; set; } = 0;
}
