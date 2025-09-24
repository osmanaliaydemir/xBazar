using Application.DTOs.Common;
using Application.Services;
using Core.Entities;
using Core.Interfaces;
using System.Text;

namespace Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<byte[]> ExportOrdersCsvAsync(DateTime from, DateTime to, Guid? storeId = null)
    {
        var orders = await _uow.Orders.GetAllAsync(o => o.CreatedAt >= from && o.CreatedAt <= to && !o.IsDeleted && (!storeId.HasValue || o.StoreId == storeId.Value));
        var sb = new StringBuilder();
        sb.AppendLine("OrderNumber,StoreId,Status,TotalAmount,CreatedAt");
        foreach (var o in orders.OrderBy(o => o.CreatedAt))
        {
            sb.AppendLine($"{o.OrderNumber},{o.StoreId},{o.Status},{o.TotalAmount},{o.CreatedAt:O}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<PagedResult<RevenueRow>> GetRevenueAsync(DateTime from, DateTime to, Guid? storeId = null, int page = 1, int pageSize = 50)
    {
        var orders = await _uow.Orders.GetAllAsync(o => o.CreatedAt >= from && o.CreatedAt <= to && !o.IsDeleted && (!storeId.HasValue || o.StoreId == storeId.Value));
        var payments = await _uow.Payments.GetAllAsync(p => !p.IsDeleted);
        var byDate = orders.GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueRow
            {
                Date = g.Key,
                Gross = g.Sum(o => o.TotalAmount),
                Refunds = payments.Where(p => p.OrderId == null ? false : g.Any(o => o.Id == p.OrderId) && p.RefundAmount.HasValue).Sum(p => p.RefundAmount ?? 0)
            })
            .OrderBy(r => r.Date)
            .ToList();

        var total = byDate.Count;
        var items = byDate.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<RevenueRow> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<byte[]> ExportAuditCsvAsync(DateTime from, DateTime to, string? table = null, string? userEmail = null)
    {
        var audits = await _uow.AuditLogs.GetAllAsync(a => a.Timestamp >= from && a.Timestamp <= to && !a.IsDeleted);
        if (!string.IsNullOrWhiteSpace(table))
        {
            audits = audits.Where(a => a.TableName == table).ToList();
        }
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            audits = audits.Where(a => a.UserEmail == userEmail).ToList();
        }
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Table,RecordId,Action,UserEmail,IpAddress,UserAgent,OldValues,NewValues");
        foreach (var a in audits.OrderBy(a => a.Timestamp))
        {
            sb.AppendLine($"{a.Timestamp:O},{a.TableName},{a.RecordId},{a.Action},{a.UserEmail},{a.IpAddress},\"{(a.UserAgent ?? string.Empty).Replace("\"","\"\"")}\",\"{(a.OldValues ?? string.Empty).Replace("\"","\"\"")}\",\"{(a.NewValues ?? string.Empty).Replace("\"","\"\"")}\"");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
