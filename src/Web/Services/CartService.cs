using Application.DTOs.Cart;
using Application.DTOs.Product;
using Application.DTOs.Common;

namespace Web.Services;

public class CartService
{
    private readonly ApiService _apiService;
    private CartDto? _currentCart;

    public CartService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public event Action? OnCartChanged;

    public CartDto? CurrentCart => _currentCart;

    public async Task<ApiResponse<CartDto>> GetCartAsync(string sessionId)
    {
        var response = await _apiService.GetAsync<CartDto>($"cart/{sessionId}");
        
        if (response.IsSuccess && response.Data != null)
        {
            _currentCart = response.Data;
            OnCartChanged?.Invoke();
        }
        
        return response;
    }

    public async Task<ApiResponse<CartDto>> AddToCartAsync(string sessionId, Guid productId, int quantity = 1)
    {
        var request = new
        {
            ProductId = productId,
            Quantity = quantity
        };

        var response = await _apiService.PostAsync<CartDto>($"cart/{sessionId}/items", request);
        
        if (response.IsSuccess && response.Data != null)
        {
            _currentCart = response.Data;
            OnCartChanged?.Invoke();
        }
        
        return response;
    }

    public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(string sessionId, Guid productId, int quantity)
    {
        var request = new
        {
            Quantity = quantity
        };

        var response = await _apiService.PutAsync<CartDto>($"cart/{sessionId}/items/{productId}", request);
        
        if (response.IsSuccess && response.Data != null)
        {
            _currentCart = response.Data;
            OnCartChanged?.Invoke();
        }
        
        return response;
    }

    public async Task<ApiResponse<CartDto>> RemoveFromCartAsync(string sessionId, Guid productId)
    {
        var response = await _apiService.DeleteAsync<CartDto>($"cart/{sessionId}/items/{productId}");
        
        if (response.IsSuccess && response.Data != null)
        {
            _currentCart = response.Data;
            OnCartChanged?.Invoke();
        }
        
        return response;
    }

    public async Task<ApiResponse<CartDto>> ClearCartAsync(string sessionId)
    {
        var response = await _apiService.DeleteAsync<CartDto>($"cart/{sessionId}");
        
        if (response.IsSuccess && response.Data != null)
        {
            _currentCart = response.Data;
            OnCartChanged?.Invoke();
        }
        
        return response;
    }

    public async Task<ApiResponse<CartDto>> MergeCartsAsync(string sessionId, Guid userId)
    {
        var response = await _apiService.PostAsync<CartDto>($"cart/{sessionId}/merge/{userId}", new { });
        
        if (response.IsSuccess && response.Data != null)
        {
            _currentCart = response.Data;
            OnCartChanged?.Invoke();
        }
        
        return response;
    }

    public int GetTotalItems()
    {
        return _currentCart?.Items?.Sum(i => i.Quantity) ?? 0;
    }

    public decimal GetTotalAmount()
    {
        return _currentCart?.TotalAmount ?? 0;
    }

    public decimal GetTotalTax()
    {
        return _currentCart?.TaxAmount ?? 0;
    }

    public decimal GetTotalShipping()
    {
        return _currentCart?.ShippingAmount ?? 0;
    }

    public decimal GetGrandTotal()
    {
        return _currentCart?.TotalAmount ?? 0;
    }
}
