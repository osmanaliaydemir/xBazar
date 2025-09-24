using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShippingController : BaseController
{
    private readonly IShippingService _shipping;

    public ShippingController(IShippingService shipping)
    {
        _shipping = shipping;
    }

    [HttpPost("orders/{orderId}/label")]
    public async Task<IActionResult> CreateLabel(Guid orderId, [FromQuery] string carrier)
    {
        var label = await _shipping.CreateLabelAsync(orderId, carrier);
        return Ok(new { label });
    }

    [HttpPost("stores/{storeId}/manifest")]
    public async Task<IActionResult> CreateManifest(Guid storeId, [FromQuery] DateTime date, [FromQuery] string carrier)
    {
        var manifest = await _shipping.CreateManifestAsync(storeId, date, carrier);
        return Ok(new { manifest });
    }

    [HttpPost("stores/{storeId}/pickup")]
    public async Task<IActionResult> SchedulePickup(Guid storeId, [FromQuery] DateTime pickupAt, [FromQuery] string carrier)
    {
        var ok = await _shipping.SchedulePickupAsync(storeId, pickupAt, carrier);
        return Ok(new { success = ok });
    }
}
