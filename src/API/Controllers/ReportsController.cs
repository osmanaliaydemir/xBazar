using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Common;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : BaseController
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("orders/export")]
    public async Task<IActionResult> ExportOrders([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? storeId = null)
    {
        var bytes = await _reports.ExportOrdersCsvAsync(from, to, storeId);
        return File(bytes, "text/csv", $"orders_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? storeId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var data = await _reports.GetRevenueAsync(from, to, storeId, page, pageSize);
        return Ok(ApiResponse.Success(data));
    }

    [HttpGet("audit/export")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ExportAudit([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? table = null, [FromQuery] string? userEmail = null)
    {
        var bytes = await _reports.ExportAuditCsvAsync(from, to, table, userEmail);
        return File(bytes, "text/csv", $"audit_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
    }
}
