using Application.DTOs.Role;

namespace Application.Services;

public interface IRoleService
{
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
    Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(Guid id);
    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<List<RoleDto>> GetActiveRolesAsync();
    Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId);
    Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    Task<List<RoleDto>> GetUserRolesAsync(Guid userId);
    Task<bool> AssignPermissionToRoleAsync(Guid roleId, string permission);
    Task<bool> RemovePermissionFromRoleAsync(Guid roleId, string permission);
    Task<List<string>> GetRolePermissionsAsync(Guid roleId);
    Task<bool> SyncRolePermissionsAsync(Guid roleId, List<string> permissions);
}
