using Application.DTOs.Order;

namespace Application.Services;

public interface ICheckoutService
{
    Task<bool> SaveGuestAddressAsync(string sessionId, GuestAddressDto address);
    Task<List<ShippingOptionDto>> GetShippingOptionsAsync(string sessionId);
    Task<bool> SelectShippingAsync(string sessionId, string shippingOptionId);
    Task<CheckoutSummaryDto> GetSummaryAsync(string sessionId);
    Task<OrderDto> FinalizeGuestAsync(string sessionId, PaymentRequestDto payment);
}

