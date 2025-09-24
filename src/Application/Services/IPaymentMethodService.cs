using Application.DTOs.Payment;

namespace Application.Services;

public interface IPaymentMethodService
{
    Task<List<PaymentMethodDto>> GetMyAsync(Guid userId);
    Task<PaymentMethodDto> GetByIdAsync(Guid userId, Guid id);
    Task<PaymentMethodDto> CreateAsync(Guid userId, CreatePaymentMethodDto dto);
    Task<PaymentMethodDto> UpdateAsync(Guid userId, Guid id, UpdatePaymentMethodDto dto);
    Task<bool> DeleteAsync(Guid userId, Guid id);
    Task<bool> SetDefaultAsync(Guid userId, Guid id);
}
