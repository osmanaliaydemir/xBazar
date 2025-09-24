using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Role;

public class UpdateRoleRequest
{
    [Required(ErrorMessage = "Role name is required")]
    [MinLength(2, ErrorMessage = "Role name must be at least 2 characters")]
    [MaxLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string> Permissions { get; set; } = new();
}
