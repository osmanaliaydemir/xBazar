using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Message : BaseEntity
{
    [Required]
    public Guid ThreadId { get; set; }
    
    [Required]
    public Guid SenderId { get; set; }
    
    [Required]
    public Guid ReceiverId { get; set; }
    
    [Required]
    public MessageType Type { get; set; } = MessageType.Text;
    
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
    
    [MaxLength(100)]
    public string? AttachmentFileName { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }
    
    public bool IsDeletedBySender { get; set; } = false;
    
    public bool IsDeletedByReceiver { get; set; } = false;
    
    // Navigation properties
    public virtual MessageThread Thread { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
    public virtual User Receiver { get; set; } = null!;
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    System = 3
}
