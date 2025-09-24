using Application.DTOs.Cart;
using Application.DTOs.Common;

namespace Application.Services;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetCartAsync(string sessionId);
    Task<ApiResponse<CartDto>> GetUserCartAsync(Guid userId);
    Task<ApiResponse<CartDto>> AddItemAsync(string sessionId, AddCartItemDto addItemDto);
    Task<ApiResponse<CartDto>> AddUserItemAsync(Guid userId, AddCartItemDto addItemDto);
    Task<ApiResponse<CartDto>> UpdateItemAsync(string sessionId, Guid productId, int quantity);
    Task<ApiResponse<CartDto>> UpdateUserItemAsync(Guid userId, Guid productId, int quantity);
    Task<ApiResponse<CartDto>> RemoveItemAsync(string sessionId, Guid productId);
    Task<ApiResponse<CartDto>> RemoveUserItemAsync(Guid userId, Guid productId);
    Task<ApiResponse<bool>> ClearCartAsync(string sessionId);
    Task<ApiResponse<bool>> ClearUserCartAsync(Guid userId);
    Task<ApiResponse<CartDto>> ApplyCouponAsync(string sessionId, string couponCode);
    Task<ApiResponse<CartDto>> ApplyUserCouponAsync(Guid userId, string couponCode);
    Task<ApiResponse<CartDto>> RemoveCouponAsync(string sessionId);
    Task<ApiResponse<CartDto>> RemoveUserCouponAsync(Guid userId);
    Task<ApiResponse<bool>> MergeCartsAsync(string sessionId, Guid userId);
    
    // ETag-aware methods for concurrency control
    Task<ApiResponse<CartDto>> UpdateItemWithETagAsync(string sessionId, Guid productId, int quantity, string? expectedETag = null);
    Task<ApiResponse<CartDto>> UpdateUserItemWithETagAsync(Guid userId, Guid productId, int quantity, string? expectedETag = null);
    Task<ApiResponse<CartDto>> RemoveItemWithETagAsync(string sessionId, Guid productId, string? expectedETag = null);
    Task<ApiResponse<CartDto>> RemoveUserItemWithETagAsync(Guid userId, Guid productId, string? expectedETag = null);
}