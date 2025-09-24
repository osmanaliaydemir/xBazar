using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ApiKey;

public class CreateApiKeyRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(50, ErrorMessage = "Environment cannot exceed 50 characters")]
    public string? Environment { get; set; }
}
