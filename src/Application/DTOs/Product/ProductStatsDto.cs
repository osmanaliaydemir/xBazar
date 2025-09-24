namespace Application.DTOs.Product;

public class ProductStatsDto
{
    public Guid ProductId { get; set; }
    public int ViewCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public int StockQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}
