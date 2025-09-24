using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product;

public class UpdateProductImageDto
{
    [Required(ErrorMessage = "Image URL is required")]
    [Url(ErrorMessage = "Invalid image URL format")]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Alt text cannot exceed 200 characters")]
    public string? AltText { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Sort order must be greater than or equal to 0")]
    public int SortOrder { get; set; } = 0;

    public bool IsMain { get; set; } = false;
}
