using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product;

public class CreateProductReviewDto
{
    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MinLength(2, ErrorMessage = "Title must be at least 2 characters")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Comment is required")]
    [MinLength(10, ErrorMessage = "Comment must be at least 10 characters")]
    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string Comment { get; set; } = string.Empty;

    public bool IsVerified { get; set; } = false;
}
