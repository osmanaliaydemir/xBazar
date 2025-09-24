using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Services;
using Core.Entities;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FavoritesController : BaseController
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromQuery] FavoriteType type, [FromQuery] Guid itemId)
    {
        var userId = GetCurrentUserId();
        var ok = await _favoriteService.AddAsync(userId, type, itemId);
        return Ok(new { success = ok });
    }

    [HttpDelete]
    public async Task<IActionResult> Remove([FromQuery] FavoriteType type, [FromQuery] Guid itemId)
    {
        var userId = GetCurrentUserId();
        var ok = await _favoriteService.RemoveAsync(userId, type, itemId);
        return Ok(new { success = ok });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] FavoriteType type)
    {
        var userId = GetCurrentUserId();
        var ids = await _favoriteService.GetUserFavoritesAsync(userId, type);
        return Ok(ids);
    }
}

