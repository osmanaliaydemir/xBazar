using Application.Services;
using Application.DTOs.Cart;

namespace Infrastructure.Services;

public class TaxService : ITaxService
{
    public Task<decimal> CalculateCartTaxAsync(CartDto cart, Guid? shippingAddressId = null)
    {
        // Basit KDV örneği: ürünlerin vergilendirilebilir olanları için %18
        var taxableSubtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        var tax = Math.Round(taxableSubtotal * 0.18m, 2);
        return Task.FromResult(tax);
    }
}


