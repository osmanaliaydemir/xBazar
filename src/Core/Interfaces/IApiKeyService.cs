using Core.Entities;

namespace Core.Interfaces;

public interface IApiKeyService
{
    Task<ApiKey?> GetApiKeyAsync(string key);
    Task<ApiKey> CreateApiKeyAsync(string name, string? description = null, Guid? userId = null, DateTime? expiresAt = null, string? environment = null);
    Task<bool> ValidateApiKeyAsync(string key);
    Task<bool> RevokeApiKeyAsync(string key);
    Task<bool> UpdateLastUsedAsync(string key);
    Task<List<ApiKey>> GetUserApiKeysAsync(Guid userId);
    Task<bool> DeleteApiKeyAsync(string key);
    Task<bool> IsApiKeyActiveAsync(string key);
    string GenerateApiKey();
}
