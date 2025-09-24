using Application.DTOs.Store;

namespace Application.Services;

public interface IStoreService
{
    // Core CRUD operations
    Task<StoreDto> GetByIdAsync(Guid id);
    Task<List<StoreDto>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<List<StoreDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<StoreDto> CreateAsync(CreateStoreDto request);
    Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto request);
    Task<bool> DeleteAsync(Guid id);
    
    // Store management
    Task<bool> ActivateAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> RejectAsync(Guid id, string reason);
    Task<bool> SuspendAsync(Guid id, string reason);
    Task<bool> UnsuspendAsync(Guid id);
    
    // User management
    Task<bool> AddUserAsync(Guid storeId, Guid userId, string role);
    Task<bool> RemoveUserAsync(Guid storeId, Guid userId);
    Task<List<StoreUserDto>> GetStoreUsersAsync(Guid storeId);
    Task<List<StoreDto>> GetUserStoresAsync(Guid userId);
    
    // Store settings
    Task<bool> UpdateSettingsAsync(Guid id, StoreSettingsDto settings);
    Task<StoreSettingsDto> GetSettingsAsync(Guid id);
    
    // Store statistics
    Task<StoreStatsDto> GetStatsAsync(Guid id);
    Task<List<StoreDto>> GetTopStoresAsync(int count = 10);
    Task<List<StoreDto>> GetNewStoresAsync(int count = 10);
    
    // Store verification
    Task<bool> RequestVerificationAsync(Guid id);
    Task<bool> VerifyStoreAsync(Guid id, string verificationCode);
}