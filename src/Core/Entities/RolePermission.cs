using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class RolePermission : BaseEntity
{
    [Required]
    public Guid RoleId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Permission { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
}
