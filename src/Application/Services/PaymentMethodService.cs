using Application.DTOs.Payment;
using Core.Entities;
using Core.Interfaces;
using Core.Exceptions;

namespace Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IUnitOfWork _uow;

    public PaymentMethodService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<PaymentMethodDto>> GetMyAsync(Guid userId)
    {
        var methods = await _uow.PaymentMethods.GetAllAsync(m => m.UserId == userId && !m.IsDeleted);
        return methods.Select(Map).ToList();
    }

    public async Task<PaymentMethodDto> GetByIdAsync(Guid userId, Guid id)
    {
        var m = await _uow.PaymentMethods.GetAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);
        if (m == null) throw new NotFoundException("Payment method not found");
        return Map(m);
    }

    public async Task<PaymentMethodDto> CreateAsync(Guid userId, CreatePaymentMethodDto dto)
    {
        var entity = new PaymentMethod
        {
            UserId = userId,
            Provider = string.IsNullOrWhiteSpace(dto.Provider) ? "iyzico" : dto.Provider,
            Token = dto.Token,
            Last4 = dto.Last4,
            Brand = dto.Brand,
            ExpiryMonth = dto.ExpiryMonth,
            ExpiryYear = dto.ExpiryYear,
            Label = dto.Label,
            IsDefault = dto.IsDefault
        };

        if (entity.IsDefault)
        {
            var all = await _uow.PaymentMethods.GetAllAsync(m => m.UserId == userId && !m.IsDeleted);
            foreach (var pm in all.Where(x => x.IsDefault))
            {
                pm.IsDefault = false;
                await _uow.PaymentMethods.UpdateAsync(pm);
            }
        }

        await _uow.PaymentMethods.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<PaymentMethodDto> UpdateAsync(Guid userId, Guid id, UpdatePaymentMethodDto dto)
    {
        var m = await _uow.PaymentMethods.GetAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);
        if (m == null) throw new NotFoundException("Payment method not found");
        if (!string.IsNullOrWhiteSpace(dto.Label)) m.Label = dto.Label;
        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            var all = await _uow.PaymentMethods.GetAllAsync(x => x.UserId == userId && !x.IsDeleted);
            foreach (var pm in all.Where(x => x.IsDefault))
            {
                pm.IsDefault = false;
                await _uow.PaymentMethods.UpdateAsync(pm);
            }
            m.IsDefault = true;
        }
        await _uow.PaymentMethods.UpdateAsync(m);
        await _uow.SaveChangesAsync();
        return Map(m);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var m = await _uow.PaymentMethods.GetAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);
        if (m == null) throw new NotFoundException("Payment method not found");
        m.IsDeleted = true;
        await _uow.PaymentMethods.UpdateAsync(m);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetDefaultAsync(Guid userId, Guid id)
    {
        var m = await _uow.PaymentMethods.GetAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);
        if (m == null) throw new NotFoundException("Payment method not found");
        var all = await _uow.PaymentMethods.GetAllAsync(x => x.UserId == userId && !x.IsDeleted);
        foreach (var pm in all.Where(x => x.IsDefault))
        {
            pm.IsDefault = false;
            await _uow.PaymentMethods.UpdateAsync(pm);
        }
        m.IsDefault = true;
        await _uow.PaymentMethods.UpdateAsync(m);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static PaymentMethodDto Map(PaymentMethod m)
    {
        return new PaymentMethodDto
        {
            Id = m.Id,
            Provider = m.Provider,
            Token = m.Token,
            Last4 = m.Last4,
            Brand = m.Brand,
            ExpiryMonth = m.ExpiryMonth,
            ExpiryYear = m.ExpiryYear,
            Label = m.Label,
            IsDefault = m.IsDefault
        };
    }
}
