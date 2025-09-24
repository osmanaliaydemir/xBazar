using Application.DTOs.Cart;
using Application.DTOs.Common;
using Core.Interfaces;
using Application.Services;

namespace Application.Services;

public class CartService : ICartService
{
    private readonly ICacheService _cache;
    private readonly IProductService _productService;
    private readonly ITaxService _taxService;
    private readonly IShippingService _shippingService;
    private readonly IDistributedLockService _lockService;
    private const string CartKey = "cart:";

    public CartService(ICacheService cache, IProductService productService, ITaxService taxService, IShippingService shippingService, IDistributedLockService lockService)
    {
        _cache = cache;
        _productService = productService;
        _taxService = taxService;
        _shippingService = shippingService;
        _lockService = lockService;
    }

    public async Task<ApiResponse<CartDto>> GetCartAsync(string sessionId)
    {
        var cart = await _cache.GetAsync<CartDto>($"{CartKey}{sessionId}") ?? NewCart(sessionId, null);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> GetUserCartAsync(Guid userId)
    {
        var cart = await _cache.GetAsync<CartDto>($"{CartKey}user:{userId}") ?? NewCart(string.Empty, userId);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> AddItemAsync(string sessionId, AddCartItemDto addItemDto)
    {
        var cart = (await GetCartAsync(sessionId)).Data!;
        await AddOrUpdateWithProductAsync(cart, addItemDto.ProductId, addItemDto.Quantity);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> AddUserItemAsync(Guid userId, AddCartItemDto addItemDto)
    {
        var cart = (await GetUserCartAsync(userId)).Data!;
        await AddOrUpdateWithProductAsync(cart, addItemDto.ProductId, addItemDto.Quantity);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> UpdateItemAsync(string sessionId, Guid productId, int quantity)
    {
        var cart = (await GetCartAsync(sessionId)).Data!;
        await AddOrUpdateWithProductAsync(cart, productId, quantity, true);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> UpdateUserItemAsync(Guid userId, Guid productId, int quantity)
    {
        var cart = (await GetUserCartAsync(userId)).Data!;
        await AddOrUpdateWithProductAsync(cart, productId, quantity, true);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> RemoveItemAsync(string sessionId, Guid productId)
    {
        var cart = (await GetCartAsync(sessionId)).Data!;
        cart.Items = cart.Items.Where(i => i.ProductId != productId).ToList();
        await RecalculateAsync(cart);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> RemoveUserItemAsync(Guid userId, Guid productId)
    {
        var cart = (await GetUserCartAsync(userId)).Data!;
        cart.Items = cart.Items.Where(i => i.ProductId != productId).ToList();
        await RecalculateAsync(cart);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<bool>> ClearCartAsync(string sessionId)
    {
        await _cache.RemoveAsync($"{CartKey}{sessionId}");
        return ApiResponse.Success(true);
    }

    public async Task<ApiResponse<bool>> ClearUserCartAsync(Guid userId)
    {
        await _cache.RemoveAsync($"{CartKey}user:{userId}");
        return ApiResponse.Success(true);
    }

    public async Task<ApiResponse<CartDto>> ApplyCouponAsync(string sessionId, string couponCode)
    {
        var cart = (await GetCartAsync(sessionId)).Data!;
        cart.CouponCode = couponCode;
        cart.CouponDiscount = 0; // placeholder
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> ApplyUserCouponAsync(Guid userId, string couponCode)
    {
        var cart = (await GetUserCartAsync(userId)).Data!;
        cart.CouponCode = couponCode;
        cart.CouponDiscount = 0; // placeholder
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> RemoveCouponAsync(string sessionId)
    {
        var cart = (await GetCartAsync(sessionId)).Data!;
        cart.CouponCode = null;
        cart.CouponDiscount = null;
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> RemoveUserCouponAsync(Guid userId)
    {
        var cart = (await GetUserCartAsync(userId)).Data!;
        cart.CouponCode = null;
        cart.CouponDiscount = null;
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<bool>> MergeCartsAsync(string sessionId, Guid userId)
    {
        var lockKey = $"cart_merge:{userId}";
        using var lockHandle = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));
        
        if (lockHandle == null)
        {
            return ApiResponse.Error<bool>("Unable to acquire lock for cart merge operation");
        }

        try
        {
            // Get fresh copies of both carts
            var guestResponse = await GetCartAsync(sessionId);
            var userResponse = await GetUserCartAsync(userId);
            
            if (!guestResponse.IsSuccess || !userResponse.IsSuccess)
            {
                return ApiResponse.Error<bool>("Failed to retrieve carts for merging");
            }

            var guest = guestResponse.Data!;
            var user = userResponse.Data!;

            // Check if user cart has been modified since last read (ETag check)
            var userCartKey = $"{CartKey}user:{userId}";
            var currentUserCart = await _cache.GetAsync<CartDto>(userCartKey);
            if (currentUserCart != null && currentUserCart.ETag != user.ETag)
            {
                return ApiResponse.Error<bool>("User cart has been modified by another operation. Please refresh and try again.");
            }

            // Merge guest items into user cart
            foreach (var item in guest.Items)
            {
                await AddOrUpdateWithProductAsync(user, item.ProductId, item.Quantity);
            }

            // Update version and ETag
            user.Version++;
            user.ETag = Guid.NewGuid().ToString();
            
            await RecalculateAsync(user);
            await SaveCartAsync(user);
            await ClearCartAsync(sessionId);
            
            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            return ApiResponse.Error<bool>($"Cart merge failed: {ex.Message}");
        }
    }

    private static CartDto NewCart(string sessionId, Guid? userId)
    {
        return new CartDto
        {
            SessionId = sessionId,
            UserId = userId,
            Items = new List<CartItemDto>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ETag = Guid.NewGuid().ToString(),
            Version = 1
        };
    }

    private async Task AddOrUpdateWithProductAsync(CartDto cart, Guid productId, int quantity, bool replace = false)
    {
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing == null)
        {
            var p = await _productService.GetByIdAsync(productId);
            cart.Items.Add(new CartItemDto
            {
                ProductId = productId,
                ProductName = p.Name,
                ProductSKU = p.SKU,
                UnitPrice = p.Price,
                Quantity = quantity,
                TotalPrice = Math.Round(p.Price * quantity, 2),
                StoreId = p.StoreId,
                StoreName = p.StoreName
            });
        }
        else
        {
            existing.Quantity = replace ? quantity : existing.Quantity + quantity;
            var p = await _productService.GetByIdAsync(productId);
            existing.UnitPrice = p.Price;
            existing.TotalPrice = Math.Round(existing.UnitPrice * existing.Quantity, 2);
        }
        await RecalculateAsync(cart);
    }

    private async Task RecalculateAsync(CartDto cart)
    {
        // Subtotal
        cart.SubTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

        // Shipping estimate (store bazlı basit yaklaşım: ilk ürünün mağazası)
        var totalWeight = 0m; // İleride ürün ağırlıklarından hesaplanabilir
        var totalItems = cart.Items.Sum(i => i.Quantity);
        var storeId = cart.Items.FirstOrDefault()?.StoreId ?? Guid.Empty;
        cart.ShippingAmount = storeId == Guid.Empty
            ? 0
            : await _shippingService.EstimateCartShippingAsync(storeId, totalWeight, totalItems);

        // Tax
        cart.TaxAmount = await _taxService.CalculateCartTaxAsync(cart);

        // Total
        cart.TotalAmount = cart.SubTotal + cart.ShippingAmount + cart.TaxAmount - cart.DiscountAmount - (cart.CouponDiscount ?? 0);
        cart.UpdatedAt = DateTime.UtcNow;
        
        // Update ETag and version for concurrency control
        cart.ETag = Guid.NewGuid().ToString();
        cart.Version++;
    }

    private async Task SaveCartAsync(CartDto cart)
    {
        var key = cart.UserId.HasValue ? $"{CartKey}user:{cart.UserId}" : $"{CartKey}{cart.SessionId}";
        await _cache.SetAsync(key, cart, TimeSpan.FromHours(12));
    }

    // ETag-aware methods for concurrency control
    public async Task<ApiResponse<CartDto>> UpdateItemWithETagAsync(string sessionId, Guid productId, int quantity, string? expectedETag = null)
    {
        var cart = (await GetCartAsync(sessionId)).Data!;
        
        if (!string.IsNullOrEmpty(expectedETag) && cart.ETag != expectedETag)
        {
            return ApiResponse.Error<CartDto>("Cart has been modified by another operation. Please refresh and try again.");
        }

        await AddOrUpdateWithProductAsync(cart, productId, quantity, true);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> UpdateUserItemWithETagAsync(Guid userId, Guid productId, int quantity, string? expectedETag = null)
    {
        var cart = (await GetUserCartAsync(userId)).Data!;
        
        if (!string.IsNullOrEmpty(expectedETag) && cart.ETag != expectedETag)
        {
            return ApiResponse.Error<CartDto>("Cart has been modified by another operation. Please refresh and try again.");
        }

        await AddOrUpdateWithProductAsync(cart, productId, quantity, true);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> RemoveItemWithETagAsync(string sessionId, Guid productId, string? expectedETag = null)
    {
        var cart = (await GetCartAsync(sessionId)).Data!;
        
        if (!string.IsNullOrEmpty(expectedETag) && cart.ETag != expectedETag)
        {
            return ApiResponse.Error<CartDto>("Cart has been modified by another operation. Please refresh and try again.");
        }

        cart.Items = cart.Items.Where(i => i.ProductId != productId).ToList();
        await RecalculateAsync(cart);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }

    public async Task<ApiResponse<CartDto>> RemoveUserItemWithETagAsync(Guid userId, Guid productId, string? expectedETag = null)
    {
        var cart = (await GetUserCartAsync(userId)).Data!;
        
        if (!string.IsNullOrEmpty(expectedETag) && cart.ETag != expectedETag)
        {
            return ApiResponse.Error<CartDto>("Cart has been modified by another operation. Please refresh and try again.");
        }

        cart.Items = cart.Items.Where(i => i.ProductId != productId).ToList();
        await RecalculateAsync(cart);
        await SaveCartAsync(cart);
        return ApiResponse.Success(cart);
    }
}
