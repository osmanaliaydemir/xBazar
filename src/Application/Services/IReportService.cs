using Application.DTOs.Common;

namespace Application.Services;

public interface IReportService
{
    Task<byte[]> ExportOrdersCsvAsync(DateTime from, DateTime to, Guid? storeId = null);
    Task<PagedResult<RevenueRow>> GetRevenueAsync(DateTime from, DateTime to, Guid? storeId = null, int page = 1, int pageSize = 50);
    Task<byte[]> ExportAuditCsvAsync(DateTime from, DateTime to, string? table = null, string? userEmail = null);
}

public class RevenueRow
{
    public DateTime Date { get; set; }
    public decimal Gross { get; set; }
    public decimal Refunds { get; set; }
    public decimal Net => Gross - Refunds;
}
