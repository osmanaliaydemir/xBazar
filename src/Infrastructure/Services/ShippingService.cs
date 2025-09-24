using Application.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ShippingService : IShippingService
{
    private readonly ILogger<ShippingService> _logger;

    public ShippingService(ILogger<ShippingService> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateLabelAsync(Guid orderId, string carrier)
    {
        _logger.LogInformation("Creating label for order {OrderId} via {Carrier}", orderId, carrier);
        await Task.Delay(200);
        return $"LBL-{carrier}-{orderId.ToString().Substring(0, 8)}";
    }

    public async Task<string> CreateManifestAsync(Guid storeId, DateTime date, string carrier)
    {
        _logger.LogInformation("Creating manifest for store {StoreId} ({Date}) via {Carrier}", storeId, date, carrier);
        await Task.Delay(200);
        return $"MNF-{carrier}-{storeId.ToString().Substring(0, 8)}-{date:yyyyMMdd}";
    }

    public async Task<bool> SchedulePickupAsync(Guid storeId, DateTime pickupAt, string carrier)
    {
        _logger.LogInformation("Scheduling pickup for store {StoreId} at {Pickup} via {Carrier}", storeId, pickupAt, carrier);
        await Task.Delay(200);
        return true;
    }

    public Task<decimal> EstimateCartShippingAsync(Guid storeId, decimal totalWeight, int totalItems, string? optionId = null)
    {
        // Örnek: temel fiyat + ağırlık ve adet bazlı artış
        var basePrice = 29.90m;
        if (string.Equals(optionId, "exp", StringComparison.OrdinalIgnoreCase))
        {
            basePrice = 59.90m;
        }

        var weightSurcharge = (decimal)Math.Ceiling((double)totalWeight) * 5m; // kg başına 5 TL
        var itemSurcharge = Math.Max(0, totalItems - 3) * 2m; // 3 adetten sonrası adet başı 2 TL
        var price = basePrice + weightSurcharge + itemSurcharge;
        return Task.FromResult(Math.Round(price, 2));
    }
}
