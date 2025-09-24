namespace Application.DTOs.Cart;

public class CartDto
{
    public string SessionId { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal? CouponDiscount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ETag { get; set; } = string.Empty; // Optimistic concurrency control
    public int Version { get; set; } = 1; // Version for conflict detection
}

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ProductAttributes { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

public class AddCartItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? ProductAttributes { get; set; }
}
