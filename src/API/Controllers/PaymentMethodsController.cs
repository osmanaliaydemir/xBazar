using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Payment;
using Application.DTOs.Common;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : BaseController
{
    private readonly IPaymentMethodService _service;

    public PaymentMethodsController(IPaymentMethodService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = GetCurrentUserId();
        var data = await _service.GetMyAsync(userId);
        return Ok(ApiResponse.Success(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var data = await _service.GetByIdAsync(userId, id);
            return Ok(ApiResponse.Success(data));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentMethodDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token)) return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "Token is required" }));
        var userId = GetCurrentUserId();
        var data = await _service.CreateAsync(userId, dto);
        return Ok(ApiResponse.Success(data));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentMethodDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var data = await _service.UpdateAsync(userId, id, dto);
            return Ok(ApiResponse.Success(data));
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
        var ok = await _service.DeleteAsync(userId, id);
        if (!ok) return HandleResult(ApiResponse.NotFound<object>("Payment method not found"));
        return Ok(ApiResponse.Success(true));
    }

    [HttpPost("{id}/default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var userId = GetCurrentUserId();
        var ok = await _service.SetDefaultAsync(userId, id);
        if (!ok) return HandleResult(ApiResponse.NotFound<object>("Payment method not found"));
        return Ok(ApiResponse.Success(true));
    }
}
