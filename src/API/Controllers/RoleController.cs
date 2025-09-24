using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Role;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Require authentication
public class RoleController : BaseController
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Policy = "UsersRead")]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving roles" });
        }
    }

    [HttpGet("active")]
    [Authorize(Policy = "UsersRead")]
    public async Task<IActionResult> GetActiveRoles()
    {
        try
        {
            var roles = await _roleService.GetActiveRolesAsync();
            return Ok(roles);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving active roles" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "UsersRead")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }
            return Ok(role);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving role" });
        }
    }

    [HttpPost]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating role" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var role = await _roleService.UpdateRoleAsync(id, request);
            return Ok(role);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating role" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "UsersDelete")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Role not found" });
            }
            return Ok(new { message = "Role deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while deleting role" });
        }
    }

    [HttpPost("{roleId}/assign/{userId}")]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> AssignRoleToUser(Guid roleId, Guid userId)
    {
        try
        {
            var result = await _roleService.AssignRoleToUserAsync(userId, roleId);
            if (!result)
            {
                return BadRequest(new { message = "Failed to assign role to user" });
            }
            return Ok(new { message = "Role assigned successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while assigning role" });
        }
    }

    [HttpDelete("{roleId}/remove/{userId}")]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid roleId, Guid userId)
    {
        try
        {
            var result = await _roleService.RemoveRoleFromUserAsync(userId, roleId);
            if (!result)
            {
                return BadRequest(new { message = "Failed to remove role from user" });
            }
            return Ok(new { message = "Role removed successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while removing role" });
        }
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "UsersRead")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        try
        {
            var roles = await _roleService.GetUserRolesAsync(userId);
            return Ok(roles);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user roles" });
        }
    }

    [HttpGet("{roleId}/permissions")]
    [Authorize(Policy = "UsersRead")]
    public async Task<IActionResult> GetRolePermissions(Guid roleId)
    {
        try
        {
            var permissions = await _roleService.GetRolePermissionsAsync(roleId);
            return Ok(permissions);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving role permissions" });
        }
    }

    [HttpPost("{roleId}/permissions")]
    [Authorize(Policy = "UsersWrite")]
    public async Task<IActionResult> SyncRolePermissions(Guid roleId, [FromBody] SyncPermissionsRequest request)
    {
        try
        {
            var result = await _roleService.SyncRolePermissionsAsync(roleId, request.Permissions);
            if (!result)
            {
                return BadRequest(new { message = "Failed to sync role permissions" });
            }
            return Ok(new { message = "Role permissions synced successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while syncing role permissions" });
        }
    }
}

public class SyncPermissionsRequest
{
    public List<string> Permissions { get; set; } = new();
}
