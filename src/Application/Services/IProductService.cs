using Application.DTOs.Product;

namespace Application.Services;

public interface IProductService
{
    // Core CRUD operations
    Task<ProductDto> GetByIdAsync(Guid id);
    Task<List<ProductDto>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<List<ProductDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<ProductDto> CreateAsync(CreateProductDto request);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto request);
    Task<bool> DeleteAsync(Guid id);
    
    // Product management
    Task<bool> ActivateAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> PublishAsync(Guid id);
    Task<bool> UnpublishAsync(Guid id);
    Task<bool> UpdateStockAsync(Guid id, int quantity);
    Task<bool> UpdatePriceAsync(Guid id, decimal price);
    
    // Category management
    Task<List<ProductDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10);
    Task<List<ProductDto>> GetByStoreAsync(Guid storeId, int page = 1, int pageSize = 10);
    Task<List<ProductDto>> GetFeaturedAsync(int count = 10);
    Task<List<ProductDto>> GetNewAsync(int count = 10);
    Task<List<ProductDto>> GetTopSellingAsync(int count = 10);
    
    // Product attributes
    Task<bool> AddAttributeAsync(Guid productId, CreateProductAttributeDto attribute);
    Task<bool> UpdateAttributeAsync(Guid productId, Guid attributeId, UpdateProductAttributeDto attribute);
    Task<bool> RemoveAttributeAsync(Guid productId, Guid attributeId);
    Task<List<ProductAttributeDto>> GetAttributesAsync(Guid productId);
    
    // Product images
    Task<bool> AddImageAsync(Guid productId, CreateProductImageDto image);
    Task<bool> UpdateImageAsync(Guid productId, Guid imageId, UpdateProductImageDto image);
    Task<bool> RemoveImageAsync(Guid productId, Guid imageId);
    Task<List<ProductImageDto>> GetImagesAsync(Guid productId);
    Task<bool> SetMainImageAsync(Guid productId, Guid imageId);
    
    // Product reviews
    Task<List<ProductReviewDto>> GetReviewsAsync(Guid productId, int page = 1, int pageSize = 10);
    Task<ProductReviewDto> AddReviewAsync(Guid productId, CreateProductReviewDto review);
    Task<bool> UpdateReviewAsync(Guid productId, Guid reviewId, UpdateProductReviewDto review);
    Task<bool> DeleteReviewAsync(Guid productId, Guid reviewId);
    
    // Product statistics
    Task<ProductStatsDto> GetStatsAsync(Guid id);
    Task<List<ProductDto>> GetLowStockAsync(int threshold = 10);
    Task<List<ProductDto>> GetOutOfStockAsync();

    // Advanced search
    Task<ProductSearchResultDto> SearchAdvancedAsync(ProductSearchQueryDto query);
}