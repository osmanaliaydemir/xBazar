using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class UserRole : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid RoleId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
