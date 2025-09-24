using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs.Common;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "Users_Read")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetAllAsync(page, pageSize);
        return HandleResult(ApiResponse.Success(result));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Users_Read")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        return HandleResult(ApiResponse.Success(result));
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        try
        {
            var data = await _userService.GetByEmailAsync(email);
            return Ok(ApiResponse.Success(data));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving user" });
        }
    }

    [HttpPost]
    [Authorize(Policy = "Users_Write")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
    {
        var result = await _userService.CreateAsync(createUserDto);
        return HandleResult(ApiResponse.Success(result));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Users_Write")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        var result = await _userService.UpdateAsync(id, updateUserDto);
        return HandleResult(ApiResponse.Success(result));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Users_Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteAsync(id);
        return HandleResult(ApiResponse.Success(result));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ok = await _userService.ChangePasswordAsync(userId, changePasswordDto);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("User not found"));
            }
            return Ok(ApiResponse.Success(true, "Password changed successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return HandleResult(ApiResponse.Unauthorized<object>(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = GetCurrentUserId();
            var data = await _userService.GetByIdAsync(userId);
            return Ok(ApiResponse.Success(data));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving profile" });
        }
    }

    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var ok = await _userService.UpdateProfileAsync(userId, request);
            if (!ok)
            {
                return HandleResult(ApiResponse.NotFound<object>("User not found"));
            }
            return Ok(ApiResponse.Success(true));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating profile" });
        }
    }
}
