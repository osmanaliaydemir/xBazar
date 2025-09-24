using Application.DTOs.Store;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Core.Exceptions;
using System.Text.RegularExpressions;

namespace Application.Services;

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public StoreService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<StoreDto> GetByIdAsync(Guid id)
    {
        var cacheKey = $"store:{id}";
        var cachedStore = await _cacheService.GetAsync<StoreDto>(cacheKey);
        
        if (cachedStore != null)
        {
            return cachedStore;
        }

        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            throw new NotFoundException("Store not found");
        }

        var storeDto = await MapToStoreDtoAsync(store);
        
        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, storeDto, TimeSpan.FromMinutes(5));

        return storeDto;
    }

    public async Task<List<StoreDto>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync(s => !s.IsDeleted);
        var pagedStores = stores
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var storeDtos = new List<StoreDto>();
        foreach (var store in pagedStores)
        {
            storeDtos.Add(await MapToStoreDtoAsync(store));
        }

        return storeDtos;
    }

    public async Task<List<StoreDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync(s => 
            !s.IsDeleted && 
            ((s.Name != null && s.Name.Contains(searchTerm)) || 
             (s.Description != null && s.Description.Contains(searchTerm)) || 
             (s.Email != null && s.Email.Contains(searchTerm))));

        var pagedStores = stores.Skip((page - 1) * pageSize).Take(pageSize);

        var storeDtos = new List<StoreDto>();
        foreach (var store in pagedStores)
        {
            storeDtos.Add(await MapToStoreDtoAsync(store));
        }

        return storeDtos;
    }

    public async Task<StoreDto> CreateAsync(CreateStoreDto request)
    {
        // Check if store name already exists
        var existingStore = await _unitOfWork.Stores.GetAsync(s => 
            s.Name == request.Name || s.Email == request.Email);
        
        if (existingStore != null)
        {
            throw new InvalidOperationException("Store with this name or email already exists");
        }

        // Generate slug from name
        var slug = GenerateSlug(request.Name);

        var store = new Store
        {
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            Email = request.Email,
            Phone = request.Phone,
            Website = request.Website,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            LogoUrl = request.LogoUrl,
            BannerUrl = request.BannerUrl,
            Status = Core.Entities.StoreStatus.Pending,
            IsVerified = false,
            IsActive = true,
            OwnerId = Guid.NewGuid() // TODO: Get from current user context
        };

        await _unitOfWork.Stores.AddAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{store.Id}");

        return await MapToStoreDtoAsync(store);
    }

    public async Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto request)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            throw new ArgumentException("Store not found");
        }

        // Check if name or email is already taken by another store
        var existingStore = await _unitOfWork.Stores.GetAsync(s => 
            (s.Name == request.Name || s.Email == request.Email) && 
            s.Id != id && !s.IsDeleted);
        
        if (existingStore != null)
        {
            throw new InvalidOperationException("Store name or email is already taken by another store");
        }

        store.Name = request.Name;
        store.Description = request.Description;
        store.Slug = GenerateSlug(request.Name);
        store.Email = request.Email;
        store.Phone = request.Phone;
        store.Website = request.Website;
        store.Address = request.Address;
        store.City = request.City;
        store.State = request.State;
        store.PostalCode = request.PostalCode;
        store.Country = request.Country;
        store.LogoUrl = request.LogoUrl;
        store.BannerUrl = request.BannerUrl;
        store.IsActive = request.IsActive;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return await MapToStoreDtoAsync(store);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        // Soft delete
        store.IsDeleted = true;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        store.IsActive = true;
        store.Status = Core.Entities.StoreStatus.Active;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        store.IsActive = false;
        store.Status = Core.Entities.StoreStatus.Inactive;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> ApproveAsync(Guid id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        store.Status = Core.Entities.StoreStatus.Active;
        store.IsActive = true;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> RejectAsync(Guid id, string reason)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        store.Status = Core.Entities.StoreStatus.Rejected;
        store.IsActive = false;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> SuspendAsync(Guid id, string reason)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        store.Status = Core.Entities.StoreStatus.Suspended;
        store.IsActive = false;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> UnsuspendAsync(Guid id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        store.Status = Core.Entities.StoreStatus.Active;
        store.IsActive = true;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> AddUserAsync(Guid storeId, Guid userId, string role)
    {
        // Check if store exists
        var store = await _unitOfWork.Stores.GetByIdAsync(storeId);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        // Check if user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Check if user is already assigned to this store
        var existingStoreUser = await _unitOfWork.StoreUsers.GetAsync(su => 
            su.StoreId == storeId && su.UserId == userId);
        
        if (existingStoreUser != null)
        {
            return true; // Already assigned
        }

        var storeUser = new StoreUser
        {
            StoreId = storeId,
            UserId = userId,
            Role = Enum.Parse<StoreUserRole>(role, true),
            IsActive = true
        };

        await _unitOfWork.StoreUsers.AddAsync(storeUser);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{storeId}");

        return true;
    }

    public async Task<bool> RemoveUserAsync(Guid storeId, Guid userId)
    {
        var storeUser = await _unitOfWork.StoreUsers.GetAsync(su => 
            su.StoreId == storeId && su.UserId == userId);
        
        if (storeUser == null)
        {
            return false;
        }

        await _unitOfWork.StoreUsers.DeleteAsync(storeUser);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{storeId}");

        return true;
    }

    public async Task<List<StoreUserDto>> GetStoreUsersAsync(Guid storeId)
    {
        var storeUsers = await _unitOfWork.StoreUsers.GetAllAsync(su => su.StoreId == storeId);
        var storeUserDtos = new List<StoreUserDto>();

        foreach (var storeUser in storeUsers)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(storeUser.UserId);
            if (user != null)
            {
                storeUserDtos.Add(new StoreUserDto
                {
                    Id = storeUser.Id,
                    StoreId = storeUser.StoreId,
                    UserId = storeUser.UserId,
                    UserName = user.UserName,
                    UserEmail = user.Email,
                    Role = storeUser.Role.ToString(),
                    IsActive = storeUser.IsActive,
                    JoinedAt = storeUser.CreatedAt
                });
            }
        }

        return storeUserDtos;
    }

    public async Task<List<StoreDto>> GetUserStoresAsync(Guid userId)
    {
        var storeUsers = await _unitOfWork.StoreUsers.GetAllAsync(su => su.UserId == userId);
        var storeIds = storeUsers.Select(su => su.StoreId).ToList();
        var stores = await _unitOfWork.Stores.GetAllAsync(s => storeIds.Contains(s.Id) && !s.IsDeleted);

        var storeDtos = new List<StoreDto>();
        foreach (var store in stores)
        {
            storeDtos.Add(await MapToStoreDtoAsync(store));
        }

        return storeDtos;
    }

    public async Task<bool> UpdateSettingsAsync(Guid id, StoreSettingsDto settings)
    {
        // TODO: Implement store settings update
        await Task.CompletedTask;
        return true;
    }

    public async Task<StoreSettingsDto> GetSettingsAsync(Guid id)
    {
        // TODO: Implement store settings retrieval
        await Task.CompletedTask;
        return new StoreSettingsDto { StoreId = id };
    }

    public async Task<StoreStatsDto> GetStatsAsync(Guid id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            throw new ArgumentException("Store not found");
        }

        // TODO: Implement actual statistics calculation
        // This would involve querying orders, products, reviews, etc.
        return new StoreStatsDto
        {
            StoreId = id,
            StoreName = store.Name,
            TotalProducts = 0,
            ActiveProducts = 0,
            TotalOrders = 0,
            PendingOrders = 0,
            CompletedOrders = 0,
            CancelledOrders = 0,
            TotalSales = 0,
            MonthlySales = 0,
            DailySales = 0,
            TotalCustomers = 0,
            NewCustomers = 0,
            TotalReviews = 0,
            AverageRating = 0,
            TotalViews = 0,
            MonthlyViews = 0,
            DailyViews = 0,
            LastOrderDate = DateTime.UtcNow,
            LastProductUpdate = DateTime.UtcNow
        };
    }

    public async Task<List<StoreDto>> GetTopStoresAsync(int count = 10)
    {
        // TODO: Implement top stores based on sales, ratings, etc.
        var stores = await _unitOfWork.Stores.GetAllAsync(s => !s.IsDeleted && s.IsActive);
        var pagedStores = stores.OrderByDescending(s => s.CreatedAt).Take(count);

        var storeDtos = new List<StoreDto>();
        foreach (var store in pagedStores)
        {
            storeDtos.Add(await MapToStoreDtoAsync(store));
        }

        return storeDtos;
    }

    public async Task<List<StoreDto>> GetNewStoresAsync(int count = 10)
    {
        var stores = await _unitOfWork.Stores.GetAllAsync(s => !s.IsDeleted && s.IsActive);
        var pagedStores = stores.OrderByDescending(s => s.CreatedAt).Take(count);

        var storeDtos = new List<StoreDto>();
        foreach (var store in pagedStores)
        {
            storeDtos.Add(await MapToStoreDtoAsync(store));
        }

        return storeDtos;
    }

    public async Task<bool> RequestVerificationAsync(Guid id)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        store.Status = Core.Entities.StoreStatus.UnderReview;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    public async Task<bool> VerifyStoreAsync(Guid id, string verificationCode)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(id);
        if (store == null || store.IsDeleted)
        {
            return false;
        }

        // TODO: Implement actual verification code validation
        // For now, we'll just mark as verified
        store.IsVerified = true;
        store.Status = Core.Entities.StoreStatus.Active;
        store.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Stores.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"store:{id}");

        return true;
    }

    private async Task<StoreDto> MapToStoreDtoAsync(Store store)
    {
        // Get store owner
        var owner = await _unitOfWork.Users.GetByIdAsync(store.OwnerId);

        // Get store users
        var storeUsers = await _unitOfWork.StoreUsers.GetAllAsync(su => su.StoreId == store.Id);
        var storeUserDtos = new List<StoreUserDto>();

        foreach (var storeUser in storeUsers)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(storeUser.UserId);
            if (user != null)
            {
                storeUserDtos.Add(new StoreUserDto
                {
                    Id = storeUser.Id,
                    StoreId = storeUser.StoreId,
                    UserId = storeUser.UserId,
                    UserName = user.UserName,
                    UserEmail = user.Email,
                    Role = storeUser.Role.ToString(),
                    IsActive = storeUser.IsActive,
                    JoinedAt = storeUser.CreatedAt
                });
            }
        }

        // TODO: Calculate actual statistics
        var productCount = 0;
        var orderCount = 0;
        var totalSales = 0m;
        var rating = 0m;
        var reviewCount = 0;

        return new StoreDto
        {
            Id = store.Id,
            Name = store.Name,
            Description = store.Description,
            Slug = store.Slug,
            LogoUrl = store.LogoUrl ?? string.Empty,
            BannerUrl = store.BannerUrl ?? string.Empty,
            Email = store.Email,
            Phone = store.Phone,
            Website = store.Website ?? string.Empty,
            Address = store.Address,
            City = store.City,
            State = store.State,
            PostalCode = store.PostalCode,
            Country = store.Country,
            Status = (Application.DTOs.Store.StoreStatus)store.Status,
            IsVerified = store.IsVerified,
            IsActive = store.IsActive,
            CreatedAt = store.CreatedAt,
            UpdatedAt = store.UpdatedAt,
            OwnerId = store.OwnerId,
            OwnerName = owner?.UserName ?? string.Empty,
            ProductCount = productCount,
            OrderCount = orderCount,
            TotalSales = totalSales,
            Rating = rating,
            ReviewCount = reviewCount,
            Users = storeUserDtos
        };
    }

    private string GenerateSlug(string name)
    {
        // Convert to lowercase and replace spaces with hyphens
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove special characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove multiple consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // Remove leading and trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }
}
