using Application.DTOs.Address;
using Core.Entities;
using Core.Interfaces;

namespace Application.Services;

public class AddressService : IAddressService
{
    private readonly IUnitOfWork _uow;

    public AddressService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<AddressDto>> GetMyAddressesAsync(Guid userId)
    {
        var addresses = await _uow.Addresses.GetAllAsync(a => a.UserId == userId && !a.IsDeleted);
        return addresses.Select(MapToDto).ToList();
    }

    public async Task<AddressDto> GetByIdAsync(Guid userId, Guid addressId)
    {
        var address = await _uow.Addresses.GetAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);
        if (address == null) throw new ArgumentException("Address not found");
        return MapToDto(address);
    }

    public async Task<AddressDto> CreateAsync(Guid userId, CreateAddressDto dto)
    {
        if (dto.IsDefault)
        {
            var myAddresses = await _uow.Addresses.GetAllAsync(a => a.UserId == userId && !a.IsDeleted);
            foreach (var addr in myAddresses.Where(a => a.IsDefault))
            {
                addr.IsDefault = false;
                await _uow.Addresses.UpdateAsync(addr);
            }
        }

        var entity = new Address
        {
            UserId = userId,
            Title = dto.Title,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            IsDefault = dto.IsDefault,
            IsActive = true
        };
        await _uow.Addresses.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<AddressDto> UpdateAsync(Guid userId, Guid addressId, UpdateAddressDto dto)
    {
        var address = await _uow.Addresses.GetAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);
        if (address == null) throw new ArgumentException("Address not found");

        address.Title = dto.Title;
        address.AddressLine1 = dto.AddressLine1;
        address.AddressLine2 = dto.AddressLine2;
        address.City = dto.City;
        address.State = dto.State;
        address.PostalCode = dto.PostalCode;
        address.Country = dto.Country;

        if (dto.IsDefault && !address.IsDefault)
        {
            var myAddresses = await _uow.Addresses.GetAllAsync(a => a.UserId == userId && !a.IsDeleted);
            foreach (var addr in myAddresses.Where(a => a.IsDefault))
            {
                addr.IsDefault = false;
                await _uow.Addresses.UpdateAsync(addr);
            }
            address.IsDefault = true;
        }
        else if (!dto.IsDefault && address.IsDefault)
        {
            address.IsDefault = false;
        }

        await _uow.Addresses.UpdateAsync(address);
        await _uow.SaveChangesAsync();
        return MapToDto(address);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid addressId)
    {
        var address = await _uow.Addresses.GetAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);
        if (address == null) return false;
        address.IsDeleted = true;
        await _uow.Addresses.UpdateAsync(address);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetDefaultAsync(Guid userId, Guid addressId)
    {
        var address = await _uow.Addresses.GetAsync(a => a.Id == addressId && a.UserId == userId && !a.IsDeleted);
        if (address == null) return false;
        var myAddresses = await _uow.Addresses.GetAllAsync(a => a.UserId == userId && !a.IsDeleted);
        foreach (var addr in myAddresses.Where(a => a.IsDefault))
        {
            addr.IsDefault = false;
            await _uow.Addresses.UpdateAsync(addr);
        }
        address.IsDefault = true;
        await _uow.Addresses.UpdateAsync(address);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static AddressDto MapToDto(Address a)
    {
        return new AddressDto
        {
            Id = a.Id,
            UserId = a.UserId,
            Title = a.Title,
            AddressLine1 = a.AddressLine1,
            AddressLine2 = a.AddressLine2,
            City = a.City,
            State = a.State,
            PostalCode = a.PostalCode,
            Country = a.Country,
            IsDefault = a.IsDefault
        };
    }
}
