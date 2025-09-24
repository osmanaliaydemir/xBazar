using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Cart;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs.Common;

namespace API.Controllers;

[Route("api/[controller]")]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] string? sessionId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.GetUserCartAsync(userId);
            return HandleResult(result);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            var result = await _cartService.GetCartAsync(sessionId);
            return HandleResult(result);
        }
        else
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Session ID required for guest users" }));
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemDto addItemDto, [FromQuery] string? sessionId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.AddUserItemAsync(userId, addItemDto);
            return HandleResult(result);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            var result = await _cartService.AddItemAsync(sessionId, addItemDto);
            return HandleResult(result);
        }
        else
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Session ID required for guest users" }));
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemDto updateItemDto, [FromQuery] string? sessionId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.UpdateUserItemAsync(userId, updateItemDto.ProductId, updateItemDto.Quantity);
            return HandleResult(result);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            var result = await _cartService.UpdateItemAsync(sessionId, updateItemDto.ProductId, updateItemDto.Quantity);
            return HandleResult(result);
        }
        else
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Session ID required for guest users" }));
        }
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveItem([FromQuery] Guid productId, [FromQuery] string? sessionId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.RemoveUserItemAsync(userId, productId);
            return HandleResult(result);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            var result = await _cartService.RemoveItemAsync(sessionId, productId);
            return HandleResult(result);
        }
        else
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Session ID required for guest users" }));
        }
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart([FromQuery] string? sessionId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.ClearUserCartAsync(userId);
            return HandleResult(result);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            var result = await _cartService.ClearCartAsync(sessionId);
            return HandleResult(result);
        }
        else
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Session ID required for guest users" }));
        }
    }

    [HttpPost("apply-coupon")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponDto applyCouponDto, [FromQuery] string? sessionId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.ApplyUserCouponAsync(userId, applyCouponDto.CouponCode);
            return HandleResult(result);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            var result = await _cartService.ApplyCouponAsync(sessionId, applyCouponDto.CouponCode);
            return HandleResult(result);
        }
        else
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Session ID required for guest users" }));
        }
    }

    [HttpDelete("remove-coupon")]
    public async Task<IActionResult> RemoveCoupon([FromQuery] string? sessionId)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.RemoveUserCouponAsync(userId);
            return HandleResult(result);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            var result = await _cartService.RemoveCouponAsync(sessionId);
            return HandleResult(result);
        }
        else
        {
            return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Session ID required for guest users" }));
        }
    }

    [HttpPost("merge")]
    [Authorize]
    public async Task<IActionResult> MergeCarts([FromBody] MergeCartsDto mergeCartsDto)
    {
        var userId = GetCurrentUserId();
        var result = await _cartService.MergeCartsAsync(mergeCartsDto.SessionId, userId);
        return HandleResult(result);
    }
}

public class UpdateCartItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ApplyCouponDto
{
    public string CouponCode { get; set; } = string.Empty;
}

public class MergeCartsDto
{
    public string SessionId { get; set; } = string.Empty;
}
