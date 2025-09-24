using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Store : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? LogoUrl { get; set; }
    
    [MaxLength(500)]
    public string? BannerUrl { get; set; }
    
    [MaxLength(200)]
    public string? Website { get; set; }
    
    [MaxLength(200)]
    public string? FacebookUrl { get; set; }
    
    [MaxLength(200)]
    public string? InstagramUrl { get; set; }
    
    [MaxLength(200)]
    public string? TwitterUrl { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Country { get; set; } = "Turkey";
    
    public StoreStatus Status { get; set; } = StoreStatus.Pending;
    
    public bool IsVerified { get; set; } = false;
    
    public DateTime? ApprovedAt { get; set; }
    
    public string? RejectionReason { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string TaxNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string IBAN { get; set; } = string.Empty;
    
    public decimal CommissionRate { get; set; } = 0.05m; // %5 default
    
    public bool IsActive { get; set; } = true;
    
    [Required]
    public Guid OwnerId { get; set; }
    
    // Navigation properties
    public virtual ICollection<StoreUser> StoreUsers { get; set; } = new List<StoreUser>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}

public enum StoreStatus
{
    Pending = 0,
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Rejected = 4,
    UnderReview = 5
}
