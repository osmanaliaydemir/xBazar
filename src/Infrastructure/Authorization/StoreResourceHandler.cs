using Microsoft.AspNetCore.Authorization;
using Core.Interfaces;
using Core.Constants;
using System.Security.Claims;

namespace Infrastructure.Authorization;

public class StoreResourceHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public StoreResourceHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Fail();
            return;
        }

        // Check if user has global permission
        var hasGlobalPermission = await HasPermissionAsync(userId, requirement.Permission);
        if (hasGlobalPermission)
        {
            context.Succeed(requirement);
            return;
        }

        // Check store-specific permission
        if (requirement.Resource != null && Guid.TryParse(requirement.Resource, out var storeId))
        {
            var hasStorePermission = await HasStorePermissionAsync(userId, storeId, requirement.Permission);
            if (hasStorePermission)
            {
                context.Succeed(requirement);
                return;
            }
        }

        context.Fail();
    }

    private async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var cacheKey = $"user_permissions:{userId}";
        var cachedPermissions = await _cacheService.GetAsync<HashSet<string>>(cacheKey);
        
        if (cachedPermissions != null)
        {
            return cachedPermissions.Contains(permission);
        }

        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == userId);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        
        if (!roleIds.Any())
        {
            return false;
        }

        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync(rp => roleIds.Contains(rp.RoleId));
        var permissions = rolePermissions.Select(rp => rp.Permission).ToHashSet();

        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(5));

        return permissions.Contains(permission);
    }

    private async Task<bool> HasStorePermissionAsync(Guid userId, Guid storeId, string permission)
    {
        // Check if user is assigned to the store
        var storeUser = await _unitOfWork.StoreUsers.GetAsync(su => su.UserId == userId && su.StoreId == storeId && su.IsActive);
        if (storeUser == null)
        {
            return false;
        }

        // Map store role to permissions
        var storePermissions = GetStoreRolePermissions(storeUser.Role);
        return storePermissions.Contains(permission);
    }

    private static HashSet<string> GetStoreRolePermissions(Core.Entities.StoreUserRole role)
    {
        return role switch
        {
            Core.Entities.StoreUserRole.Owner => Roles.PermissionMappings.RolePermissions[Roles.StoreOwner].ToHashSet(),
            Core.Entities.StoreUserRole.Manager => Roles.PermissionMappings.RolePermissions[Roles.StoreManager].ToHashSet(),
            Core.Entities.StoreUserRole.Support => Roles.PermissionMappings.RolePermissions[Roles.StoreSupport].ToHashSet(),
            Core.Entities.StoreUserRole.Packer => Roles.PermissionMappings.RolePermissions[Roles.StorePacker].ToHashSet(),
            _ => new HashSet<string>()
        };
    }
}
