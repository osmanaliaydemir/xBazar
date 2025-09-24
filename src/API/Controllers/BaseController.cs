using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Common;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(ApiResponse<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        if (result.StatusCode.HasValue)
        {
            return StatusCode(result.StatusCode.Value, result);
        }
        return BadRequest(result);
    }

    protected IActionResult HandleResult(ApiResponse result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        if (result.StatusCode.HasValue)
        {
            return StatusCode(result.StatusCode.Value, result);
        }
        return BadRequest(result);
    }

    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
