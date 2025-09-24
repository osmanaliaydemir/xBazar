namespace Application.DTOs.Store;

public class StoreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public StoreStatus Status { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public List<StoreUserDto> Users { get; set; } = new();
}

public class StoreUserDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class StoreSettingsDto
{
    public Guid StoreId { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Language { get; set; } = "tr-TR";
    public string TimeZone { get; set; } = "Turkey Standard Time";
    public bool AllowGuestCheckout { get; set; } = true;
    public bool RequireEmailVerification { get; set; } = true;
    public bool AllowReviews { get; set; } = true;
    public bool AllowRatings { get; set; } = true;
    public bool AllowWishlist { get; set; } = true;
    public bool AllowCompare { get; set; } = true;
    public bool AllowNewsletter { get; set; } = true;
    public bool AllowSocialLogin { get; set; } = true;
    public string Theme { get; set; } = "default";
    public string LogoUrl { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
    public string FaviconUrl { get; set; } = string.Empty;
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;
}

public class StoreStatsDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal DailySales { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalViews { get; set; }
    public int MonthlyViews { get; set; }
    public int DailyViews { get; set; }
    public DateTime LastOrderDate { get; set; }
    public DateTime LastProductUpdate { get; set; }
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