using Application.DTOs.User;

namespace Application.Services;

public interface IUserService
{
    // Core CRUD operations
    Task<UserDto> GetByIdAsync(Guid id, bool includeDeleted = false);
    Task<UserDto> GetByEmailAsync(string email);
    Task<List<UserDto>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<List<UserDto>> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<UserDto> CreateAsync(CreateUserDto request);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto request);
    Task<bool> DeleteAsync(Guid id);
    
    // User management
    Task<bool> ActivateAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> ChangePasswordAsync(Guid id, ChangePasswordDto request);
    Task<bool> UpdateProfileAsync(Guid id, UpdateProfileRequest request);
    
    // Role management
    Task<List<UserDto>> GetByRoleAsync(string roleName, int page = 1, int pageSize = 10);
    Task<bool> AssignRoleAsync(Guid userId, Guid roleId);
    Task<bool> RemoveRoleAsync(Guid userId, Guid roleId);
    
    // Store management
    Task<List<UserDto>> GetByStoreAsync(Guid storeId, int page = 1, int pageSize = 10);
    
    // Email management
    Task<bool> ConfirmEmailAsync(Guid userId, string token);
    Task<bool> ResendEmailConfirmationAsync(Guid userId);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
}