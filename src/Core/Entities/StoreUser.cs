using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class StoreUser : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public StoreUserRole Role { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Store Store { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public enum StoreUserRole
{
    Owner = 0,
    Manager = 1,
    Support = 2,
    Packer = 3
}
