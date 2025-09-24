using Application.DTOs.Role;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public RoleService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        // Check if role already exists
        var existingRole = await _unitOfWork.Roles.GetAsync(r => r.Name == request.Name);
        if (existingRole != null)
        {
            throw new InvalidOperationException("Role with this name already exists");
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = request.IsSystemRole,
            IsActive = true
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        // Assign permissions
        if (request.Permissions.Any())
        {
            await SyncRolePermissionsAsync(role.Id, request.Permissions);
        }

        // Clear cache
        await _cacheService.RemoveAsync($"role_permissions:{role.Name}");

        return await GetRoleByIdAsync(role.Id) ?? throw new InvalidOperationException("Failed to create role");
    }

    public async Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            throw new ArgumentException("Role not found");
        }

        if (role.IsSystemRole && request.Name != role.Name)
        {
            throw new InvalidOperationException("Cannot change name of system role");
        }

        role.Name = request.Name;
        role.Description = request.Description;
        role.IsActive = request.IsActive;
        role.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();

        // Update permissions
        if (request.Permissions.Any())
        {
            await SyncRolePermissionsAsync(role.Id, request.Permissions);
        }

        // Clear cache
        await _cacheService.RemoveAsync($"role_permissions:{role.Name}");

        return await GetRoleByIdAsync(role.Id) ?? throw new InvalidOperationException("Failed to update role");
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
        {
            return false;
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot delete system role");
        }

        // Check if role is assigned to any users
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.RoleId == id);
        if (userRoles.Any())
        {
            throw new InvalidOperationException("Cannot delete role that is assigned to users");
        }

        // Soft delete
        role.IsDeleted = true;
        role.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"role_permissions:{role.Name}");

        return true;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null || role.IsDeleted)
        {
            return null;
        }

        var permissions = await GetRolePermissionsAsync(role.Id);
        var userCount = await _unitOfWork.UserRoles.CountAsync(ur => ur.RoleId == id);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            Permissions = permissions,
            UserCount = userCount
        };
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync(r => !r.IsDeleted);
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissions = await GetRolePermissionsAsync(role.Id);
            var userCount = await _unitOfWork.UserRoles.CountAsync(ur => ur.RoleId == role.Id);

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Permissions = permissions,
                UserCount = userCount
            });
        }

        return roleDtos;
    }

    public async Task<List<RoleDto>> GetActiveRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync(r => !r.IsDeleted && r.IsActive);
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissions = await GetRolePermissionsAsync(role.Id);
            var userCount = await _unitOfWork.UserRoles.CountAsync(ur => ur.RoleId == role.Id);

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Permissions = permissions,
                UserCount = userCount
            });
        }

        return roleDtos;
    }

    public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId)
    {
        // Check if user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Check if role exists
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null || role.IsDeleted || !role.IsActive)
        {
            return false;
        }

        // Check if user already has this role
        var existingUserRole = await _unitOfWork.UserRoles.GetAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (existingUserRole != null)
        {
            return true; // Already assigned
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        await _unitOfWork.UserRoles.AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        // Clear user cache
        await _cacheService.RemoveAsync($"user_roles:{userId}");
        await _cacheService.RemoveAsync($"user_permissions:{userId}");

        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
    {
        var userRole = await _unitOfWork.UserRoles.GetAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole == null)
        {
            return false;
        }

        await _unitOfWork.UserRoles.DeleteAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        // Clear user cache
        await _cacheService.RemoveAsync($"user_roles:{userId}");
        await _cacheService.RemoveAsync($"user_permissions:{userId}");

        return true;
    }

    public async Task<List<RoleDto>> GetUserRolesAsync(Guid userId)
    {
        var userRoles = await _unitOfWork.UserRoles.GetAllAsync(ur => ur.UserId == userId);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Roles.GetAllAsync(r => roleIds.Contains(r.Id) && !r.IsDeleted);

        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissions = await GetRolePermissionsAsync(role.Id);
            var userCount = await _unitOfWork.UserRoles.CountAsync(ur => ur.RoleId == role.Id);

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Permissions = permissions,
                UserCount = userCount
            });
        }

        return roleDtos;
    }

    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, string permission)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null || role.IsDeleted)
        {
            return false;
        }

        // Check if permission already exists
        var existingPermission = await _unitOfWork.RolePermissions.GetAsync(rp => rp.RoleId == roleId && rp.Permission == permission);
        if (existingPermission != null)
        {
            return true; // Already assigned
        }

        var rolePermission = new RolePermission
        {
            RoleId = roleId,
            Permission = permission
        };

        await _unitOfWork.RolePermissions.AddAsync(rolePermission);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"role_permissions:{role.Name}");

        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, string permission)
    {
        var rolePermission = await _unitOfWork.RolePermissions.GetAsync(rp => rp.RoleId == roleId && rp.Permission == permission);
        if (rolePermission == null)
        {
            return false;
        }

        await _unitOfWork.RolePermissions.DeleteAsync(rolePermission);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role != null)
        {
            await _cacheService.RemoveAsync($"role_permissions:{role.Name}");
        }

        return true;
    }

    public async Task<List<string>> GetRolePermissionsAsync(Guid roleId)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null)
        {
            return new List<string>();
        }

        var cacheKey = $"role_permissions:{role.Name}";
        var cachedPermissions = await _cacheService.GetAsync<List<string>>(cacheKey);
        
        if (cachedPermissions != null)
        {
            return cachedPermissions;
        }

        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync(rp => rp.RoleId == roleId);
        var permissions = rolePermissions.Select(rp => rp.Permission).ToList();

        // Cache for 10 minutes
        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(10));

        return permissions;
    }

    public async Task<bool> SyncRolePermissionsAsync(Guid roleId, List<string> permissions)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null || role.IsDeleted)
        {
            return false;
        }

        // Remove existing permissions
        var existingPermissions = await _unitOfWork.RolePermissions.GetAllAsync(rp => rp.RoleId == roleId);
        await _unitOfWork.RolePermissions.DeleteRangeAsync(existingPermissions);

        // Add new permissions
        var newPermissions = permissions.Select(permission => new RolePermission
        {
            RoleId = roleId,
            Permission = permission
        }).ToList();

        await _unitOfWork.RolePermissions.AddRangeAsync(newPermissions);
        await _unitOfWork.SaveChangesAsync();

        // Clear cache
        await _cacheService.RemoveAsync($"role_permissions:{role.Name}");

        return true;
    }
}
