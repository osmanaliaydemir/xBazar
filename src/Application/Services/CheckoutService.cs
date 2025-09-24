using Application.DTOs.Order;
using Core.Interfaces;

namespace Application.Services;

public class CheckoutService : ICheckoutService
{
    private readonly ICacheService _cache;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CheckoutService(ICacheService cache, ICartService cartService, IOrderService orderService)
    {
        _cache = cache;
        _cartService = cartService;
        _orderService = orderService;
    }

    public async Task<bool> SaveGuestAddressAsync(string sessionId, GuestAddressDto address)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return false;
        await _cache.SetAsync($"checkout:addr:{sessionId}", address, TimeSpan.FromHours(6));
        return true;
    }

    public async Task<List<ShippingOptionDto>> GetShippingOptionsAsync(string sessionId)
    {
        // Basit örnek: sabit kargo seçenekleri
        var options = new List<ShippingOptionDto>
        {
            new ShippingOptionDto { Id = "std", Name = "Standart", Price = 49.90m, EstimatedDays = 3 },
            new ShippingOptionDto { Id = "exp", Name = "Ekspres", Price = 99.90m, EstimatedDays = 1 }
        };
        await _cache.SetAsync($"checkout:shipopts:{sessionId}", options, TimeSpan.FromHours(6));
        return options;
    }

    public async Task<bool> SelectShippingAsync(string sessionId, string shippingOptionId)
    {
        var options = await _cache.GetAsync<List<ShippingOptionDto>>($"checkout:shipopts:{sessionId}") ?? new();
        var selected = options.FirstOrDefault(o => o.Id == shippingOptionId);
        if (selected == null) return false;
        await _cache.SetAsync($"checkout:ship:{sessionId}", selected, TimeSpan.FromHours(6));
        return true;
    }

    public async Task<CheckoutSummaryDto> GetSummaryAsync(string sessionId)
    {
        var cart = await _cartService.GetCartAsync(sessionId);
        var shipping = await _cache.GetAsync<ShippingOptionDto>($"checkout:ship:{sessionId}") ?? new ShippingOptionDto { Id = "std", Name = "Standart", Price = 49.90m, EstimatedDays = 3 };

        var subtotal = cart.Data?.TotalAmount ?? 0m;
        var discount = 0m; // Kupon entegrasyonu burada uygulanacak
        var tax = Math.Round(subtotal * 0.18m, 2);
        var total = subtotal + shipping.Price + tax - discount;

        return new CheckoutSummaryDto
        {
            Subtotal = subtotal,
            Shipping = shipping.Price,
            Discount = discount,
            Tax = tax,
            Total = total
        };
    }

    public async Task<OrderDto> FinalizeGuestAsync(string sessionId, PaymentRequestDto payment)
    {
        var cart = await _cartService.GetCartAsync(sessionId);
        var address = await _cache.GetAsync<GuestAddressDto>($"checkout:addr:{sessionId}");
        if (cart.Data == null || address == null) throw new InvalidOperationException("Checkout info missing");

        var create = new CreateOrderDto
        {
            UserId = null,
            StoreId = cart.Data.Items.First().StoreId,
            ShippingAddressId = Guid.NewGuid(), // Placeholder; normalde adres entity oluşturulur
            CouponCode = null,
            Notes = null,
            OrderItems = cart.Data.Items.Select(i => new CreateOrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                ProductAttributes = null
            }).ToList(),
            GuestEmail = address.Email,
            GuestFirstName = address.FirstName,
            GuestLastName = address.LastName,
            GuestPhone = address.PhoneNumber
        };

        var orderResp = await _orderService.CreateAsync(create);
        if (!orderResp.IsSuccess || orderResp.Data == null) throw new InvalidOperationException("Order create failed");

        var payResp = await _orderService.ProcessPaymentAsync(orderResp.Data.Id, payment);
        if (!payResp.IsSuccess) throw new InvalidOperationException(payResp.Message ?? "Payment failed");

        return orderResp.Data;
    }
}

