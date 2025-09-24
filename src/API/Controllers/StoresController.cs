using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Store;
using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Application.DTOs.Common;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Require authentication
public class StoresController : BaseController
{
    private readonly IStoreService _storeService;
    private readonly IFavoriteService _favoriteService;

    public StoresController(IStoreService storeService, IFavoriteService favoriteService)
    {
        _storeService = storeService;
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var data = await _storeService.GetAllAsync(page, pageSize);
            return Ok(ApiResponse.Success(data));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving stores" });
        }
    }

    [HttpGet("search")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Search term is required" }));
            }

            var data = await _storeService.SearchAsync(searchTerm, page, pageSize);
            return Ok(ApiResponse.Success(data));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while searching stores" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var data = await _storeService.GetByIdAsync(id);
            return Ok(ApiResponse.Success(data));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving store" });
        }
    }

    [HttpPost]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Create([FromBody] CreateStoreDto request)
    {
        try
        {
            var data = await _storeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse.Success(data));
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
            return StatusCode(500, new { message = "An error occurred while creating store" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreDto request)
    {
        try
        {
            var data = await _storeService.UpdateAsync(id, request);
            return Ok(ApiResponse.Success(data));
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
            return StatusCode(500, new { message = "An error occurred while updating store" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "StoresDelete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var ok = await _storeService.DeleteAsync(id);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store deleted successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while deleting store" });
        }
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            var ok = await _storeService.ActivateAsync(id);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store activated successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while activating store" });
        }
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            var ok = await _storeService.DeactivateAsync(id);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store deactivated successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while deactivating store" });
        }
    }

    [HttpPost("{id}/approve")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            var ok = await _storeService.ApproveAsync(id);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store approved successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while approving store" });
        }
    }

    [HttpPost("{id}/reject")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectStoreRequest request)
    {
        try
        {
            var ok = await _storeService.RejectAsync(id, request.Reason);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store rejected successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while rejecting store" });
        }
    }

    [HttpPost("{id}/suspend")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendStoreRequest request)
    {
        try
        {
            var ok = await _storeService.SuspendAsync(id, request.Reason);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store suspended successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while suspending store" });
        }
    }

    [HttpPost("{id}/unsuspend")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> Unsuspend(Guid id)
    {
        try
        {
            var ok = await _storeService.UnsuspendAsync(id);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store unsuspended successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while unsuspending store" });
        }
    }

    [HttpPost("{id}/users/{userId}")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> AddUser(Guid id, Guid userId, [FromBody] AddUserToStoreRequest request)
    {
        try
        {
            var ok = await _storeService.AddUserAsync(id, userId, request.Role);
            if (!ok)
            {
                return HandleResult(ApiResponse.Error("Failed to add user to store"));
            }
            return Ok(ApiResponse.Success(true, "User added to store successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while adding user to store" });
        }
    }

    [HttpDelete("{id}/users/{userId}")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId)
    {
        try
        {
            var ok = await _storeService.RemoveUserAsync(id, userId);
            if (!ok)
            {
                return HandleResult(ApiResponse.Error("Failed to remove user from store"));
            }
            return Ok(ApiResponse.Success(true, "User removed from store successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while removing user from store" });
        }
    }

    [HttpGet("{id}/users")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetStoreUsers(Guid id)
    {
        try
        {
            var data = await _storeService.GetStoreUsersAsync(id);
            return Ok(ApiResponse.Success(data));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving store users" });
        }
    }

    [HttpGet("user/{userId}")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetUserStores(Guid userId)
    {
        try
        {
            var data = await _storeService.GetUserStoresAsync(userId);
            return Ok(ApiResponse.Success(data));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user stores" });
        }
    }

    [HttpGet("{id}/settings")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetSettings(Guid id)
    {
        try
        {
            var data = await _storeService.GetSettingsAsync(id);
            return Ok(ApiResponse.Success(data));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving store settings" });
        }
    }

    [HttpPut("{id}/settings")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> UpdateSettings(Guid id, [FromBody] StoreSettingsDto request)
    {
        try
        {
            var ok = await _storeService.UpdateSettingsAsync(id, request);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("Store not found"));
            }
            return Ok(ApiResponse.Success(true, "Store settings updated successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating store settings" });
        }
    }

    [HttpGet("{id}/stats")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetStats(Guid id)
    {
        try
        {
            var data = await _storeService.GetStatsAsync(id);
            return Ok(ApiResponse.Success(data));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving store statistics" });
        }
    }

    [HttpGet("top")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetTopStores([FromQuery] int count = 10)
    {
        try
        {
            var data = await _storeService.GetTopStoresAsync(count);
            return Ok(ApiResponse.Success(data));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving top stores" });
        }
    }

    [HttpGet("new")]
    [Authorize(Policy = "StoresRead")]
    public async Task<IActionResult> GetNewStores([FromQuery] int count = 10)
    {
        try
        {
            var data = await _storeService.GetNewStoresAsync(count);
            return Ok(ApiResponse.Success(data));
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving new stores" });
        }
    }

    [HttpPost("{id}/request-verification")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> RequestVerification(Guid id)
    {
        try
        {
            var result = await _storeService.RequestVerificationAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Store not found" });
            }
            return Ok(new { message = "Verification request submitted successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while requesting verification" });
        }
    }

    [HttpPost("{id}/verify")]
    [Authorize(Policy = "StoresWrite")]
    public async Task<IActionResult> VerifyStore(Guid id, [FromBody] VerifyStoreRequest request)
    {
        try
        {
            var result = await _storeService.VerifyStoreAsync(id, request.VerificationCode);
            if (!result)
            {
                return NotFound(new { message = "Store not found" });
            }
            return Ok(new { message = "Store verified successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while verifying store" });
        }
    }

    [HttpPost("{id}/follow")]
    public async Task<IActionResult> Follow(Guid id)
    {
        if (_favoriteService == null) return HandleResult(ApiResponse.Error("Favorites service not available"));
        var userId = GetCurrentUserId();
        var ok = await _favoriteService.AddAsync(userId, FavoriteType.Store, id);
        return Ok(ApiResponse.Success(ok));
    }

    [HttpDelete("{id}/follow")]
    public async Task<IActionResult> Unfollow(Guid id)
    {
        if (_favoriteService == null) return HandleResult(ApiResponse.Error("Favorites service not available"));
        var userId = GetCurrentUserId();
        var ok = await _favoriteService.RemoveAsync(userId, FavoriteType.Store, id);
        return Ok(ApiResponse.Success(ok));
    }

    [HttpGet("followed")]
    public async Task<IActionResult> GetFollowedStores()
    {
        if (_favoriteService == null) return HandleResult(ApiResponse.Error("Favorites service not available"));
        var userId = GetCurrentUserId();
        var ids = await _favoriteService.GetUserFavoritesAsync(userId, FavoriteType.Store);
        return Ok(ApiResponse.Success(ids));
    }
}

public class RejectStoreRequest
{
    [Required(ErrorMessage = "Reason is required")]
    public string Reason { get; set; } = string.Empty;
}

public class SuspendStoreRequest
{
    [Required(ErrorMessage = "Reason is required")]
    public string Reason { get; set; } = string.Empty;
}

public class AddUserToStoreRequest
{
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;
}

public class VerifyStoreRequest
{
    [Required(ErrorMessage = "Verification code is required")]
    public string VerificationCode { get; set; } = string.Empty;
}