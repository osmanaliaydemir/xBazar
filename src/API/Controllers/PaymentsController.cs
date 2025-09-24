using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.DTOs.Common;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : BaseController
{
    [HttpPost("webhook/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(string provider)
    {
        // TODO: Verify provider signature from headers and body
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        // Basic provider guard
        if (string.IsNullOrWhiteSpace(provider)) return HandleResult(ApiResponse.ValidationError<object>(new List<string> { "provider is required" }));
        return Ok(ApiResponse.Success(true));
    }

    [HttpGet("success")]
    [AllowAnonymous]
    public IActionResult Success([FromQuery] string? orderNumber = null)
    {
        return Ok(ApiResponse.Success(new { status = "success", orderNumber }));
    }

    [HttpGet("fail")]
    [AllowAnonymous]
    public IActionResult Fail([FromQuery] string? orderNumber = null, [FromQuery] string? error = null)
    {
        return HandleResult(ApiResponse.Error("Payment failed", error == null ? null : new List<string> { error }));
    }
}
