using System.Security.Cryptography;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public ApiKeyService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiKey?> GetApiKeyAsync(string key)
    {
        var cacheKey = $"api_key:{key}";
        var cachedApiKey = await _cacheService.GetAsync<ApiKey>(cacheKey);
        
        if (cachedApiKey != null)
        {
            return cachedApiKey;
        }

        var apiKey = await _unitOfWork.ApiKeys.GetAsync(ak => ak.Key == key && !ak.IsDeleted);
        
        if (apiKey != null)
        {
            // Cache for 5 minutes
            await _cacheService.SetAsync(cacheKey, apiKey, TimeSpan.FromMinutes(5));
        }

        return apiKey;
    }

    public async Task<ApiKey> CreateApiKeyAsync(string name, string? description = null, Guid? userId = null, DateTime? expiresAt = null, string? environment = null)
    {
        var key = GenerateApiKey();
        
        var apiKey = new ApiKey
        {
            Name = name,
            Key = key,
            Description = description,
            UserId = userId,
            IsActive = true,
            ExpiresAt = expiresAt,
            Environment = environment
        };

        await _unitOfWork.ApiKeys.AddAsync(apiKey);
        await _unitOfWork.SaveChangesAsync();

        return apiKey;
    }

    public async Task<bool> ValidateApiKeyAsync(string key)
    {
        var apiKey = await GetApiKeyAsync(key);
        
        if (apiKey == null)
            return false;

        if (!apiKey.IsActive)
            return false;

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
            return false;

        // Update last used
        await UpdateLastUsedAsync(key);

        return true;
    }

    public async Task<bool> RevokeApiKeyAsync(string key)
    {
        var apiKey = await GetApiKeyAsync(key);
        if (apiKey == null)
            return false;

        apiKey.IsActive = false;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ApiKeys.UpdateAsync(apiKey);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        var cacheKey = $"api_key:{key}";
        await _cacheService.RemoveAsync(cacheKey);

        return true;
    }

    public async Task<bool> UpdateLastUsedAsync(string key)
    {
        var apiKey = await GetApiKeyAsync(key);
        if (apiKey == null)
            return false;

        apiKey.LastUsedAt = DateTime.UtcNow;
        apiKey.UsageCount++;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ApiKeys.UpdateAsync(apiKey);
        await _unitOfWork.SaveChangesAsync();

        // Update cache
        var cacheKey = $"api_key:{key}";
        await _cacheService.SetAsync(cacheKey, apiKey, TimeSpan.FromMinutes(5));

        return true;
    }

    public async Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId)
    {
        var cacheKey = $"user_api_keys:{userId}";
        var cachedApiKeys = await _cacheService.GetAsync<List<ApiKey>>(cacheKey);
        
        if (cachedApiKeys != null)
        {
            return cachedApiKeys;
        }

        var apiKeys = await _unitOfWork.ApiKeys.GetAllAsync(ak => ak.UserId == userId && !ak.IsDeleted);
        
        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, apiKeys.ToList(), TimeSpan.FromMinutes(5));

        return apiKeys.ToList();
    }

    public async Task<bool> DeleteApiKeyAsync(string key)
    {
        var apiKey = await GetApiKeyAsync(key);
        if (apiKey == null)
            return false;

        // Soft delete
        apiKey.IsDeleted = true;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.ApiKeys.UpdateAsync(apiKey);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        var cacheKey = $"api_key:{key}";
        await _cacheService.RemoveAsync(cacheKey);

        // Clear user cache
        if (apiKey.UserId.HasValue)
        {
            await _cacheService.RemoveAsync($"user_api_keys:{apiKey.UserId.Value}");
        }

        return true;
    }

    public async Task<bool> IsApiKeyActiveAsync(string key)
    {
        var apiKey = await GetApiKeyAsync(key);
        return apiKey?.IsActive == true;
    }

    public string GenerateApiKey()
    {
        // Generate a secure random API key
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        
        // Convert to base64 and remove special characters
        var key = Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
        
        // Add prefix for identification
        return $"xbz_{key}";
    }
}
