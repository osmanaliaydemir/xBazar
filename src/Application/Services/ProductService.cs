using Application.DTOs.Product;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Core.Exceptions;
using System.Net;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public ProductService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var cacheKey = $"product:{id}";
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
        
        if (cachedProduct != null)
        {
            return cachedProduct;
        }

        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
        {
            throw new NotFoundException("Product not found");
        }

        var productDto = await MapToProductDtoAsync(product);
        
        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(5));

        return productDto;
    }

    public async Task<ProductDto> GetBySkuAsync(string sku)
    {
        var product = await _unitOfWork.Products.GetAsync(p => p.SKU == sku && !p.IsDeleted);
        if (product == null)
        {
            throw new NotFoundException("Product not found");
        }

        return await MapToProductDtoAsync(product);
    }

    public async Task<List<ProductDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => !p.IsDeleted);
        var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize);

        var productDtos = new List<ProductDto>();
        foreach (var product in pagedProducts)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }

        return productDtos;
    }

    public async Task<List<ProductDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => 
            !p.IsDeleted && 
            ((p.Name != null && p.Name.Contains(searchTerm)) || 
             (p.Description != null && p.Description.Contains(searchTerm)) || 
             (p.SKU != null && p.SKU.Contains(searchTerm))));
        var pagedProducts = products
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var productDtos = new List<ProductDto>();
        foreach (var product in pagedProducts)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }

        return productDtos;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto request)
    {
        // Check if product SKU already exists
        var existingProduct = await _unitOfWork.Products.GetAsync(p => p.SKU == request.SKU);
        
        if (existingProduct != null)
        {
            throw new ConflictException("Product with this SKU already exists", "product_conflict_sku");
        }

        // Check if store exists
        var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId);
        if (store == null || store.IsDeleted)
        {
            throw new NotFoundException("Store not found");
        }

        // Check if category exists
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null || category.IsDeleted)
        {
            throw new NotFoundException("Category not found");
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            StoreId = request.StoreId,
            CategoryId = request.CategoryId,
            Price = request.Price,
            CompareAtPrice = request.ComparePrice,
            StockQuantity = request.StockQuantity,
            TrackQuantity = request.TrackQuantity,
            AllowBackorder = request.AllowBackorder,
            Weight = request.Weight,
            Length = request.Length ?? 0,
            Width = request.Width ?? 0,
            Height = request.Height ?? 0,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return await MapToProductDtoAsync(product);
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
        {
            throw new ArgumentException("Product not found");
        }

        // Check if SKU already exists for another product
        if (product.SKU != request.SKU && await _unitOfWork.Products.ExistsAsync(p => p.SKU == request.SKU && p.Id != id))
        {
            throw new InvalidOperationException("Product with this SKU already exists");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.SKU = request.SKU;
        product.Price = request.Price;
        product.CompareAtPrice = request.ComparePrice;
        product.StockQuantity = request.StockQuantity;
        product.TrackQuantity = request.TrackQuantity;
        product.AllowBackorder = request.AllowBackorder;
        product.Weight = request.Weight;
        product.Length = request.Length ?? 0;
        product.Width = request.Width ?? 0;
        product.Height = request.Height ?? 0;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return await MapToProductDtoAsync(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
        {
            return false;
        }

        product.IsDeleted = true;
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<bool> PublishAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<bool> UnpublishAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<bool> UpdateStockAsync(Guid id, int quantity)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<bool> ReserveStockAsync(Guid id, int quantity)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        if (product.StockQuantity < quantity)
        {
            throw new InvalidOperationException("Insufficient stock");
        }

        product.ReservedQuantity += quantity;
        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<bool> ReleaseStockAsync(Guid id, int quantity)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        if (product.ReservedQuantity < quantity)
        {
            throw new InvalidOperationException("Insufficient reserved stock");
        }

        product.ReservedQuantity -= quantity;
        product.StockQuantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<List<ProductDto>> GetByStoreAsync(Guid storeId, int page = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => p.StoreId == storeId && !p.IsDeleted);
        var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize);

        var productDtos = new List<ProductDto>();
        foreach (var product in pagedProducts)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }
        return productDtos;
    }

    public async Task<List<ProductDto>> GetByCategoryAsync(Guid categoryId, int page = 1, int pageSize = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => p.CategoryId == categoryId && !p.IsDeleted);
        var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize);

        var productDtos = new List<ProductDto>();
        foreach (var product in pagedProducts)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }
        return productDtos;
    }

    public async Task<List<ProductDto>> GetFeaturedAsync(int count = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => p.IsFeatured && !p.IsDeleted);
        var pagedProducts = products.Take(count);

        var productDtos = new List<ProductDto>();
        foreach (var product in pagedProducts)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }
        return productDtos;
    }

    public async Task<List<ProductDto>> GetNewAsync(int count = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => !p.IsDeleted);
        var pagedProducts = products.OrderByDescending(p => p.CreatedAt).Take(count);

        var productDtos = new List<ProductDto>();
        foreach (var product in pagedProducts)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }
        return productDtos;
    }

    public async Task<List<ProductDto>> GetTopSellingAsync(int count = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => !p.IsDeleted);
        var pagedProducts = products.OrderByDescending(p => p.ViewCount).Take(count);

        var productDtos = new List<ProductDto>();
        foreach (var product in pagedProducts)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }
        return productDtos;
    }

    public async Task<bool> UpdatePriceAsync(Guid id, decimal price)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.Price = price;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();
        await _cacheService.RemoveByPatternAsync("product:*");
        return true;
    }

    public async Task<List<ProductDto>> GetLowStockAsync(int threshold = 10)
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => 
            !p.IsDeleted && 
            p.TrackQuantity && 
            p.StockQuantity <= threshold);

        var productDtos = new List<ProductDto>();
        foreach (var product in products)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }
        return productDtos;
    }

    public async Task<List<ProductDto>> GetOutOfStockAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => 
            !p.IsDeleted && 
            p.TrackQuantity && 
            p.StockQuantity <= 0);

        var productDtos = new List<ProductDto>();
        foreach (var product in products)
        {
            productDtos.Add(await MapToProductDtoAsync(product));
        }
        return productDtos;
    }

    public async Task<ProductStatsDto> GetStatsAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null || product.IsDeleted)
        {
            throw new ArgumentException("Product not found");
        }

        var reviews = await _unitOfWork.Reviews.GetAllAsync(r => r.ProductId == id && !r.IsDeleted);
        var orderItems = await _unitOfWork.OrderItems.GetAllAsync(oi => oi.ProductId == id);

        return new ProductStatsDto
        {
            ProductId = id,
            ViewCount = product.ViewCount,
            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            TotalSales = orderItems.Sum(oi => oi.Quantity),
            TotalRevenue = orderItems.Sum(oi => oi.TotalPrice),
            StockQuantity = product.StockQuantity,
            ReservedQuantity = product.ReservedQuantity,
            AvailableQuantity = product.StockQuantity - product.ReservedQuantity
        };
    }

    // Product Images
    public async Task<bool> AddImageAsync(Guid productId, CreateProductImageDto request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null || product.IsDeleted)
        {
            throw new ArgumentException("Product not found");
        }

        var productImage = new ProductImage
        {
            ProductId = productId,
            ImageUrl = request.ImageUrl,
            AltText = request.AltText,
            SortOrder = request.SortOrder,
            IsMain = request.IsMain,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductImages.AddAsync(productImage);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<bool> UpdateImageAsync(Guid productId, Guid imageId, UpdateProductImageDto request)
    {
        var productImage = await _unitOfWork.ProductImages.GetByIdAsync(imageId);
        if (productImage == null || productImage.IsDeleted || productImage.ProductId != productId)
        {
            throw new ArgumentException("Product image not found");
        }

        productImage.ImageUrl = request.ImageUrl;
        productImage.AltText = request.AltText;
        productImage.SortOrder = request.SortOrder;
        productImage.IsMain = request.IsMain;
        productImage.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductImages.UpdateAsync(productImage);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<bool> RemoveImageAsync(Guid productId, Guid imageId)
    {
        var productImage = await _unitOfWork.ProductImages.GetByIdAsync(imageId);
        if (productImage == null || productImage.IsDeleted || productImage.ProductId != productId)
        {
            return false;
        }

        productImage.IsDeleted = true;
        productImage.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductImages.UpdateAsync(productImage);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<List<ProductImageDto>> GetImagesAsync(Guid productId)
    {
        var productImages = await _unitOfWork.ProductImages.GetAllAsync(pi => pi.ProductId == productId && !pi.IsDeleted);
        
        return productImages.Select(pi => new ProductImageDto
        {
            Id = pi.Id,
            ProductId = pi.ProductId,
            ImageUrl = pi.ImageUrl,
            AltText = pi.AltText,
            SortOrder = pi.SortOrder,
            IsMain = pi.IsMain,
            CreatedAt = pi.CreatedAt
        }).ToList();
    }

    public async Task<bool> SetMainImageAsync(Guid productId, Guid imageId)
    {
        var productImage = await _unitOfWork.ProductImages.GetByIdAsync(imageId);
        if (productImage == null || productImage.IsDeleted || productImage.ProductId != productId)
        {
            return false;
        }

        // Set all other images as not main
        var allImages = await _unitOfWork.ProductImages.GetAllAsync(pi => pi.ProductId == productId && !pi.IsDeleted);
        foreach (var img in allImages)
        {
            img.IsMain = false;
            img.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ProductImages.UpdateAsync(img);
        }

        // Set this image as main
        productImage.IsMain = true;
        productImage.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.ProductImages.UpdateAsync(productImage);

        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    // Product Attributes
    public async Task<bool> AddAttributeAsync(Guid productId, CreateProductAttributeDto request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null || product.IsDeleted)
        {
            throw new ArgumentException("Product not found");
        }

        var productAttribute = new ProductAttribute
        {
            ProductId = productId,
            Name = request.Name,
            Value = request.Value,
            SortOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductAttributes.AddAsync(productAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<bool> UpdateAttributeAsync(Guid productId, Guid attributeId, UpdateProductAttributeDto request)
    {
        var productAttribute = await _unitOfWork.ProductAttributes.GetByIdAsync(attributeId);
        if (productAttribute == null || productAttribute.IsDeleted || productAttribute.ProductId != productId)
        {
            throw new ArgumentException("Product attribute not found");
        }

        productAttribute.Name = request.Name;
        productAttribute.Value = request.Value;
        productAttribute.SortOrder = request.SortOrder;
        productAttribute.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductAttributes.UpdateAsync(productAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<bool> RemoveAttributeAsync(Guid productId, Guid attributeId)
    {
        var productAttribute = await _unitOfWork.ProductAttributes.GetByIdAsync(attributeId);
        if (productAttribute == null || productAttribute.IsDeleted || productAttribute.ProductId != productId)
        {
            return false;
        }

        productAttribute.IsDeleted = true;
        productAttribute.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ProductAttributes.UpdateAsync(productAttribute);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<List<ProductAttributeDto>> GetAttributesAsync(Guid productId)
    {
        var productAttributes = await _unitOfWork.ProductAttributes.GetAllAsync(pa => pa.ProductId == productId && !pa.IsDeleted);
        
        return productAttributes.Select(pa => new ProductAttributeDto
        {
            Id = pa.Id,
            ProductId = pa.ProductId,
            Name = pa.Name,
            Value = pa.Value,
            SortOrder = pa.SortOrder,
            CreatedAt = pa.CreatedAt
        }).ToList();
    }

    // Product Reviews
    public async Task<ProductReviewDto> AddReviewAsync(Guid productId, CreateProductReviewDto request)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null || product.IsDeleted)
        {
            throw new ArgumentException("Product not found");
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = Guid.Empty,
            OrderId = Guid.Empty,
            Rating = request.Rating,
            Comment = request.Comment,
            IsVerifiedPurchase = request.IsVerified,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(productId);

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return new ProductReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            Rating = review.Rating,
            Title = string.Empty,
            Comment = review.Comment ?? string.Empty,
            IsVerified = review.IsVerifiedPurchase,
            IsApproved = review.IsApproved,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public async Task<bool> UpdateReviewAsync(Guid productId, Guid reviewId, UpdateProductReviewDto request)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null || review.IsDeleted || review.ProductId != productId)
        {
            throw new ArgumentException("Product review not found");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Reviews.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId);

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<bool> DeleteReviewAsync(Guid productId, Guid reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null || review.IsDeleted || review.ProductId != productId)
        {
            return false;
        }

        review.IsDeleted = true;
        review.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Reviews.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId);

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    public async Task<List<ProductReviewDto>> GetReviewsAsync(Guid productId, int page = 1, int pageSize = 10)
    {
        var reviews = await _unitOfWork.Reviews.GetAllAsync(r => r.ProductId == productId && !r.IsDeleted);
        var pagedReviews = reviews.Skip((page - 1) * pageSize).Take(pageSize);
        
        return pagedReviews.Select(r => new ProductReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserId = r.UserId,
            Rating = r.Rating,
            Title = string.Empty,
            Comment = r.Comment ?? string.Empty,
            IsVerified = r.IsVerifiedPurchase,
            IsApproved = r.IsApproved,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();
    }

    public async Task<bool> MarkReviewAsHelpfulAsync(Guid reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review == null || review.IsDeleted)
        {
            return false;
        }

        review.HelpfulCount++;
        review.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Reviews.UpdateAsync(review);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveByPatternAsync("product:*");

        return true;
    }

    private async Task<ProductDto> MapToProductDtoAsync(Product product)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(product.StoreId);
        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);
        
        var productImages = await _unitOfWork.ProductImages.GetAllAsync(pi => pi.ProductId == product.Id && !pi.IsDeleted);
        var productAttributes = await _unitOfWork.ProductAttributes.GetAllAsync(pa => pa.ProductId == product.Id && !pa.IsDeleted);
        var reviews = await _unitOfWork.Reviews.GetAllAsync(r => r.ProductId == product.Id && !r.IsDeleted);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description ?? string.Empty,
            SKU = product.SKU,
            StoreId = product.StoreId,
            CategoryId = product.CategoryId,
            Price = product.Price,
            ComparePrice = product.CompareAtPrice,
            StockQuantity = product.StockQuantity,
            TrackQuantity = product.TrackQuantity,
            AllowBackorder = product.AllowBackorder,
            Weight = product.Weight,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            ViewCount = product.ViewCount,
            Rating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            StoreName = store != null ? store.Name : string.Empty,
            CategoryName = category != null ? category.Name : string.Empty,
            Images = productImages.Select(pi => new ProductImageDto
            {
                Id = pi.Id,
                ProductId = pi.ProductId,
                ImageUrl = pi.ImageUrl,
                AltText = pi.AltText,
                SortOrder = pi.SortOrder,
                IsMain = pi.IsMain,
                CreatedAt = pi.CreatedAt
            }).ToList(),
            Attributes = productAttributes.Select(pa => new ProductAttributeDto
            {
                Id = pa.Id,
                ProductId = pa.ProductId,
                Name = pa.Name,
                Value = pa.Value,
                SortOrder = pa.SortOrder,
                CreatedAt = pa.CreatedAt
            }).ToList(),
            Reviews = reviews.Select(r => new ProductReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                Rating = r.Rating,
                Title = string.Empty,
                Comment = r.Comment ?? string.Empty,
                IsVerified = r.IsVerifiedPurchase,
                IsApproved = r.IsApproved,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList()
        };
    }

    private async Task UpdateProductRatingAsync(Guid productId)
    {
        var reviews = await _unitOfWork.Reviews.GetAllAsync(r => r.ProductId == productId && !r.IsDeleted && r.IsApproved);
        
        if (reviews.Any())
        {
            var averageRating = reviews.Average(r => r.Rating);
            var reviewCount = reviews.Count();

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product != null)
            {
                product.AverageRating = (decimal)averageRating;
                product.ReviewCount = reviewCount;
                product.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }

    public async Task<ProductSearchResultDto> SearchAdvancedAsync(ProductSearchQueryDto query)
    {
        var all = await _unitOfWork.Products.GetAllAsync(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            all = all.Where(p => (p.Name != null && p.Name.Contains(term)) || (p.Description != null && p.Description.Contains(term))).ToList();
        }
        if (query.CategoryId.HasValue)
        {
            all = all.Where(p => p.CategoryId == query.CategoryId.Value).ToList();
        }
        if (query.MinPrice.HasValue)
        {
            all = all.Where(p => p.Price >= query.MinPrice.Value).ToList();
        }
        if (query.MaxPrice.HasValue)
        {
            all = all.Where(p => p.Price <= query.MaxPrice.Value).ToList();
        }

        all = query.SortBy switch
        {
            "price_asc" => all.OrderBy(p => p.Price).ToList(),
            "price_desc" => all.OrderByDescending(p => p.Price).ToList(),
            "popular" => all.OrderByDescending(p => p.ViewCount).ToList(),
            _ => all.OrderByDescending(p => p.CreatedAt).ToList(),
        };

        var total = all.Count();
        var items = all.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        // Facets (basic)
        var categoryFacets = all.GroupBy(p => p.CategoryId)
            .Select(g => new FacetItemDto { Key = g.Key.ToString(), Label = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(f => f.Count).Take(10).ToList();
        var priceFacets = new List<FacetItemDto>
        {
            new FacetItemDto { Key = "0-1000", Label = "0-1000", Count = all.Count(p => p.Price >= 0 && p.Price <= 1000) },
            new FacetItemDto { Key = "1000-5000", Label = "1000-5000", Count = all.Count(p => p.Price > 1000 && p.Price <= 5000) },
            new FacetItemDto { Key = "5000-10000", Label = "5000-10000", Count = all.Count(p => p.Price > 5000 && p.Price <= 10000) },
            new FacetItemDto { Key = ">10000", Label = ">10000", Count = all.Count(p => p.Price > 10000) }
        };

        var dtos = new List<ProductDto>();
        foreach (var p in items)
        {
            dtos.Add(await GetByIdAsync(p.Id));
        }

        return new ProductSearchResultDto
        {
            Items = dtos,
            TotalCount = total,
            CategoryFacets = categoryFacets,
            PriceFacets = priceFacets
        };
    }
}