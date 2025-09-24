using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Order;
using Application.DTOs.Common;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CheckoutController : BaseController
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost("guest/address")] // body: GuestAddressDto, query: sessionId
    public async Task<IActionResult> SaveGuestAddress([FromBody] GuestAddressDto address, [FromQuery] string sessionId)
    {
        var ok = await _checkoutService.SaveGuestAddressAsync(sessionId, address);
        return Ok(ApiResponse.Success(ok));
    }

    [HttpGet("guest/shipping-options")] // query: sessionId
    public async Task<IActionResult> GetShippingOptions([FromQuery] string sessionId)
    {
        var options = await _checkoutService.GetShippingOptionsAsync(sessionId);
        return Ok(ApiResponse.Success(options));
    }

    [HttpPost("guest/shipping-selection")] // query: sessionId, body: SelectShippingRequest
    public async Task<IActionResult> SelectShipping([FromQuery] string sessionId, [FromBody] SelectShippingRequest request)
    {
        var ok = await _checkoutService.SelectShippingAsync(sessionId, request.ShippingOptionId);
        return Ok(ApiResponse.Success(ok));
    }

    [HttpGet("guest/summary")] // query: sessionId
    public async Task<IActionResult> GetSummary([FromQuery] string sessionId)
    {
        var summary = await _checkoutService.GetSummaryAsync(sessionId);
        return Ok(ApiResponse.Success(summary));
    }

    [HttpPost("guest/finalize")] // query: sessionId, body: PaymentRequestDto
    public async Task<IActionResult> FinalizeGuest([FromQuery] string sessionId, [FromBody] PaymentRequestDto payment)
    {
        var order = await _checkoutService.FinalizeGuestAsync(sessionId, payment);
        return Ok(ApiResponse.Success(order));
    }
}

