using Microsoft.AspNetCore.Authorization;
using Core.Interfaces;
using Core.Constants;
using System.Security.Claims;

namespace Infrastructure.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public PermissionHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
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

        // Check if user has the required permission
        var hasPermission = await HasPermissionAsync(userId, requirement.Permission, requirement.Resource);
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

    private async Task<bool> HasPermissionAsync(Guid userId, string permission, string? resource)
    {
        // Check cache first
        var cacheKey = $"user_permissions:{userId}";
        var cachedPermissions = await _cacheService.GetAsync<HashSet<string>>(cacheKey);
        
        if (cachedPermissions != null)
        {
            return cachedPermissions.Contains(permission);
        }

        // Get user roles
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == userId);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        
        if (!roleIds.Any())
        {
            return false;
        }

        // Get role permissions
        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync(rp => roleIds.Contains(rp.RoleId));
        var permissions = rolePermissions.Select(rp => rp.Permission).ToHashSet();

        // Cache permissions for 5 minutes
        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(5));

        return permissions.Contains(permission);
    }
}
