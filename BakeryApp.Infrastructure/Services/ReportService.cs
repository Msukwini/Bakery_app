using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BakeryApp.Infrastructure.Services;

public interface IReportService
{
    Task<byte[]> ExportOrdersCsvAsync(DateTime? from, DateTime? to, OrderStatus? status);
    Task<byte[]> ExportCommissionLedgerCsvAsync(Guid? resellerEmployeeId, DateTime? from, DateTime? to);
    Task<byte[]> ExportDeliveryEarningsCsvAsync(Guid? deliveryEmployeeId, DateTime? from, DateTime? to);
}

public class ReportService : IReportService
{
    private readonly BakeryDbContext _context;

    public ReportService(BakeryDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportOrdersCsvAsync(DateTime? from, DateTime? to, OrderStatus? status)
    {
        var query = _context.BuyerOrders
            .Include(o => o.Items)
            .ThenInclude(i => i.ProductVariant)
            .ThenInclude(v => v.Product)
            .AsQueryable();

        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(o => o.CreatedAt <= to.Value);
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("OrderNumber,Customer,CustomerPhone,OrderDate,Status,TotalAmount,IsPaid,Items");

        foreach (var order in orders)
        {
            var items = string.Join(";", order.Items.Select(i => $"{i.ProductVariant?.Product?.Name ?? "Unknown"} x{i.Quantity}"));
            csv.AppendLine($"{order.OrderNumber},{order.CustomerName},{order.CustomerPhone},{order.CreatedAt},{order.Status},{order.TotalAmount},{order.IsPaid},{items}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportCommissionLedgerCsvAsync(Guid? resellerEmployeeId, DateTime? from, DateTime? to)
    {
        var query = _context.CommissionLedgerEntries
            .Include(e => e.Reseller)
            .Include(e => e.ResellerSale)
                .ThenInclude(s => s.ProductVariant)
                    .ThenInclude(v => v.Product)
            .AsQueryable();

        if (resellerEmployeeId.HasValue) query = query.Where(e => e.ResellerEmployeeId == resellerEmployeeId.Value);
        if (from.HasValue) query = query.Where(e => e.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(e => e.CreatedAt <= to.Value);

        var entries = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("ResellerCode,Product,Variant,Quantity,SaleDate,AmountEarned,AmountPaid,IsSettled,CreatedAt");

        foreach (var entry in entries)
        {
            var sale = entry.ResellerSale;
            csv.AppendLine($"{entry.Reseller?.Code ?? "Unknown"},{sale?.ProductVariant?.Product?.Name ?? "Unknown"},{sale?.ProductVariant?.SizeName ?? "Unknown"},{sale?.Quantity ?? 0},{sale?.SaleDate},{entry.AmountEarned},{entry.AmountPaid},{entry.IsSettled},{entry.CreatedAt}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportDeliveryEarningsCsvAsync(Guid? deliveryEmployeeId, DateTime? from, DateTime? to)
    {
        var query = _context.DeliveryEarnings
            .Include(e => e.DeliveryEmployee)
            .AsQueryable();

        if (deliveryEmployeeId.HasValue) query = query.Where(e => e.DeliveryEmployeeId == deliveryEmployeeId.Value);
        if (from.HasValue) query = query.Where(e => e.EarningDate >= from.Value);
        if (to.HasValue) query = query.Where(e => e.EarningDate <= to.Value);

        var earnings = await query.OrderByDescending(e => e.EarningDate).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("DeliveryEmployee,Date,AmountEarned,AmountPaid,IsSettled");

        foreach (var e in earnings)
        {
            csv.AppendLine($"{e.DeliveryEmployee?.Code ?? "Unknown"},{e.EarningDate},{e.AmountEarned},{e.AmountPaid},{e.IsSettled}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
