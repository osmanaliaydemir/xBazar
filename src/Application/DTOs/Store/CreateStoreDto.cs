using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Store;

public class CreateStoreDto
{
    [Required(ErrorMessage = "Store name is required")]
    [MinLength(2, ErrorMessage = "Store name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Store name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    public string Phone { get; set; } = string.Empty;

    [Url(ErrorMessage = "Invalid website URL format")]
    public string? Website { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [MinLength(10, ErrorMessage = "Address must be at least 10 characters")]
    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [MinLength(2, ErrorMessage = "City must be at least 2 characters")]
    [MaxLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [MinLength(2, ErrorMessage = "State must be at least 2 characters")]
    [MaxLength(50, ErrorMessage = "State cannot exceed 50 characters")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal code is required")]
    [MinLength(5, ErrorMessage = "Postal code must be at least 5 characters")]
    [MaxLength(10, ErrorMessage = "Postal code cannot exceed 10 characters")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    [MinLength(2, ErrorMessage = "Country must be at least 2 characters")]
    [MaxLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
    public string Country { get; set; } = "Turkey";

    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
}
