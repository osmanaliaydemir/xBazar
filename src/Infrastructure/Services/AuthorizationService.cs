using System.Security.Claims;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public AuthorizationService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var userPermissions = await GetUserPermissionsAsync(userId);
        return userPermissions.Contains(permission);
    }

    public async Task<bool> HasRoleAsync(string userId, string role)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return userRoles.Contains(role);
    }

    public async Task<bool> HasAnyRoleAsync(string userId, params string[] roles)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return roles.Any(role => userRoles.Contains(role));
    }

    public async Task<bool> HasAllRolesAsync(string userId, params string[] roles)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return roles.All(role => userRoles.Contains(role));
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = $"user_permissions:{userId}";
        var cachedPermissions = await _cacheService.GetAsync<List<string>>(cacheKey);
        
        if (cachedPermissions != null)
        {
            return cachedPermissions;
        }

        var userRoles = await GetUserRolesAsync(userId);
        var permissions = new List<string>();

        foreach (var role in userRoles)
        {
            var rolePermissions = await GetRolePermissionsAsync(role);
            permissions.AddRange(rolePermissions);
        }

        // Remove duplicates
        permissions = permissions.Distinct().ToList();

        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(5));

        return permissions;
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var cacheKey = $"user_roles:{userId}";
        var cachedRoles = await _cacheService.GetAsync<List<string>>(cacheKey);
        
        if (cachedRoles != null)
        {
            return cachedRoles;
        }

        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == Guid.Parse(userId));
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetAllAsync(r => roleIds.Contains(r.Id) && r.IsActive && !r.IsDeleted);
        
        var roleNames = roles.Select(r => r.Name).ToList();

        // Cache for 5 minutes
        await _cacheService.SetAsync(cacheKey, roleNames, TimeSpan.FromMinutes(5));

        return roleNames;
    }

    public async Task<bool> IsInRoleAsync(ClaimsPrincipal user, string role)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await HasRoleAsync(userId, role);
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await HasPermissionAsync(userId, permission);
    }

    private async Task<List<string>> GetRolePermissionsAsync(string roleName)
    {
        var cacheKey = $"role_permissions:{roleName}";
        var cachedPermissions = await _cacheService.GetAsync<List<string>>(cacheKey);
        
        if (cachedPermissions != null)
        {
            return cachedPermissions;
        }

        var role = await _unitOfWork.Roles.GetAsync(r => r.Name == roleName && r.IsActive && !r.IsDeleted);
        if (role == null)
        {
            return new List<string>();
        }

        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync(rp => rp.RoleId == role.Id);
        var permissions = rolePermissions.Select(rp => rp.Permission).ToList();

        // Cache for 10 minutes
        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(10));

        return permissions;
    }
}
