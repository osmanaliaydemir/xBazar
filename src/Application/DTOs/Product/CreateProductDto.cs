using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Product;

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [MinLength(2, ErrorMessage = "Product name must be at least 2 characters")]
    [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Short description cannot exceed 500 characters")]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "SKU is required")]
    [MaxLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Compare price must be greater than or equal to 0")]
    public decimal? ComparePrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Cost price must be greater than or equal to 0")]
    public decimal? CostPrice { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be greater than or equal to 0")]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Min stock quantity must be greater than or equal to 0")]
    public int MinStockQuantity { get; set; } = 0;

    public bool TrackQuantity { get; set; } = true;
    public bool AllowBackorder { get; set; } = false;

    [Range(0, double.MaxValue, ErrorMessage = "Weight must be greater than or equal to 0")]
    public decimal Weight { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "Length must be greater than or equal to 0")]
    public decimal? Length { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Width must be greater than or equal to 0")]
    public decimal? Width { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Height must be greater than or equal to 0")]
    public decimal? Height { get; set; }

    [MaxLength(50, ErrorMessage = "Barcode cannot exceed 50 characters")]
    public string? Barcode { get; set; }

    [MaxLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
    public string? ISBN { get; set; }

    [MaxLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
    public string? Brand { get; set; }

    [MaxLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
    public string? Model { get; set; }

    [MaxLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
    public string? Color { get; set; }

    [MaxLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
    public string? Size { get; set; }

    [MaxLength(100, ErrorMessage = "Material cannot exceed 100 characters")]
    public string? Material { get; set; }

    public ProductCondition Condition { get; set; } = ProductCondition.New;

    public bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public bool IsDigital { get; set; } = false;
    public bool RequiresShipping { get; set; } = true;
    public bool IsTaxable { get; set; } = true;

    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
    public decimal TaxRate { get; set; } = 0;

    [MaxLength(200, ErrorMessage = "Meta title cannot exceed 200 characters")]
    public string? MetaTitle { get; set; }

    [MaxLength(500, ErrorMessage = "Meta description cannot exceed 500 characters")]
    public string? MetaDescription { get; set; }

    [MaxLength(500, ErrorMessage = "Meta keywords cannot exceed 500 characters")]
    public string? MetaKeywords { get; set; }

    [MaxLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }

    [Required(ErrorMessage = "Store ID is required")]
    public Guid StoreId { get; set; }

    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }
}
