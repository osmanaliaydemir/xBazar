namespace Application.Services;

using Application.DTOs.Cart;

public interface ITaxService
{
    Task<decimal> CalculateCartTaxAsync(CartDto cart, Guid? shippingAddressId = null);
}


