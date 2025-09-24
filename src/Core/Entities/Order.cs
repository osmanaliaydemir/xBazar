using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class Order : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public Guid? UserId { get; set; } // Nullable for guest orders
    
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    public Guid ShippingAddressId { get; set; }
    
    public Guid? BillingAddressId { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.New;
    
    [Required]
    public decimal SubTotal { get; set; }
    
    public decimal TaxAmount { get; set; } = 0;
    
    public decimal ShippingAmount { get; set; } = 0;
    
    public decimal DiscountAmount { get; set; } = 0;
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [MaxLength(50)]
    public string? CouponCode { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime? PaidAt { get; set; }
    
    public DateTime? PackedAt { get; set; }
    
    public DateTime? ShippedAt { get; set; }
    
    public DateTime? DeliveredAt { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    
    [MaxLength(1000)]
    public string? CancellationReason { get; set; }
    
    [MaxLength(100)]
    public string? TrackingNumber { get; set; }
    
    [MaxLength(100)]
    public string? ShippingCompany { get; set; }
    
    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }
    
    // Guest order fields
    [MaxLength(100)]
    public string? GuestEmail { get; set; }
    
    [MaxLength(100)]
    public string? GuestFirstName { get; set; }
    
    [MaxLength(100)]
    public string? GuestLastName { get; set; }
    
    [MaxLength(20)]
    public string? GuestPhone { get; set; }
    
    // Navigation properties
    public virtual User? User { get; set; }
    public virtual Store Store { get; set; } = null!;
    public virtual Address ShippingAddress { get; set; } = null!;
    public virtual Address? BillingAddress { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}

public enum OrderStatus
{
    New = 0,
    Paid = 1,
    Packed = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Returned = 6,
    Refunded = 7
}
