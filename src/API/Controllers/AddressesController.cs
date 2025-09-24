using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Address;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AddressesController : BaseController
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAddresses()
    {
        var userId = GetCurrentUserId();
        var result = await _addressService.GetMyAddressesAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _addressService.GetByIdAsync(userId, id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _addressService.CreateAsync(userId, dto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAddressDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _addressService.UpdateAsync(userId, id, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        var ok = await _addressService.DeleteAsync(userId, id);
        if (!ok) return NotFound(new { message = "Address not found" });
        return Ok(new { success = true });
    }

    [HttpPost("{id}/default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var userId = GetCurrentUserId();
        var ok = await _addressService.SetDefaultAsync(userId, id);
        if (!ok) return NotFound(new { message = "Address not found" });
        return Ok(new { success = true });
    }
}
