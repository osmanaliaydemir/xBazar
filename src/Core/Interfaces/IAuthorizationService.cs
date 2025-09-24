using System.Security.Claims;

namespace Core.Interfaces;

public interface IAuthorizationService
{
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<bool> HasRoleAsync(string userId, string role);
    Task<bool> HasAnyRoleAsync(string userId, params string[] roles);
    Task<bool> HasAllRolesAsync(string userId, params string[] roles);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role);
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission);
}
