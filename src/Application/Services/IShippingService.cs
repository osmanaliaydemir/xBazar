namespace Application.Services;

public interface IShippingService
{
    Task<string> CreateLabelAsync(Guid orderId, string carrier);
    Task<string> CreateManifestAsync(Guid storeId, DateTime date, string carrier);
    Task<bool> SchedulePickupAsync(Guid storeId, DateTime pickupAt, string carrier);
    Task<decimal> EstimateCartShippingAsync(Guid storeId, decimal totalWeight, int totalItems, string? optionId = null);
}
