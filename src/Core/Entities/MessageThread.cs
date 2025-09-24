using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class MessageThread : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public Guid User1Id { get; set; }
    
    [Required]
    public Guid User2Id { get; set; }
    
    public Guid? OrderId { get; set; } // Optional: related to specific order
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastMessageAt { get; set; }
    
    // Navigation properties
    public virtual User User1 { get; set; } = null!;
    public virtual User User2 { get; set; } = null!;
    public virtual Order? Order { get; set; }
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
