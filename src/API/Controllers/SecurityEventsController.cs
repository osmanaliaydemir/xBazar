using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Common;

namespace API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class SecurityEventsController : BaseController
{
    private readonly ISecurityEventService _service;

    public SecurityEventsController(ISecurityEventService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? eventType = null)
    {
        var items = await _service.GetAsync(from, to, eventType);
        return Ok(ApiResponse.Success(items));
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? eventType = null)
    {
        var bytes = await _service.ExportCsvAsync(from, to, eventType);
        return File(bytes, "text/csv", $"security_events_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
    }
}
