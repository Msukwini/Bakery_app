using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public class FinanceSummaryDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal Revenue { get; set; }
    public decimal GuestRevenue { get; set; }
    public decimal ResellerRevenue { get; set; }
    public decimal CostOfGoods { get; set; }
    public decimal Commissions { get; set; }
    public decimal DeliveryEarnings { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public int OrdersCount { get; set; }
    public int SalesCount { get; set; }
}

public class MonthlyFinanceDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public decimal Revenue { get; set; }
    public decimal CostOfGoods { get; set; }
    public decimal Commissions { get; set; }
    public decimal DeliveryEarnings { get; set; }
    public decimal NetProfit { get; set; }
}

public interface IFinancialService
{
    Task<FinanceSummaryDto> GetSummaryAsync(DateTime? from, DateTime? to);
    Task<List<MonthlyFinanceDto>> GetMonthlyBreakdownAsync(int year);
    Task<decimal> GetStockValueAsync();
}

public class FinancialService : IFinancialService
{
    private readonly BakeryDbContext _context;

    public FinancialService(BakeryDbContext context)
    {
        _context = context;
    }

    public async Task<FinanceSummaryDto> GetSummaryAsync(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.MinValue;
        var toDate = to ?? DateTime.UtcNow;

        // Revenue from guest orders (paid)
        var guestRevenue = await _context.BuyerOrders
            .Where(o => o.IsPaid && o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var ordersCount = await _context.BuyerOrders
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .CountAsync();

        // Revenue from reseller sales
        var resellerSales = await _context.ResellerSales
            .Where(s => s.SaleDate >= fromDate && s.SaleDate <= toDate)
            .Select(s => new { s.Quantity, s.UnitPriceAtSale })
            .ToListAsync();

        var resellerRevenue = resellerSales.Sum(s => s.Quantity * s.UnitPriceAtSale);
        var salesCount = resellerSales.Count;

        // COGS: sum of positive inventory cost entries, prorated by sold units
        // Simple approach: take TotalCost / TotalQuantity × SoldQuantity per variant
        var totalCogs = 0m;
        var allVariants = await _context.ProductVariants
            .Select(v => v.Id)
            .ToListAsync();

        foreach (var variantId in allVariants)
        {
            var incoming = await _context.InventoryLedgerEntries
                .Where(e => e.ProductVariantId == variantId && e.Quantity > 0 && e.TotalCost != null)
                .ToListAsync();

            if (!incoming.Any()) continue;

            var totalIncomingQty = incoming.Sum(e => e.Quantity);
            var totalIncomingCost = incoming.Sum(e => e.TotalCost!.Value);
            if (totalIncomingQty == 0) continue;

            var avgCostPerUnit = totalIncomingCost / totalIncomingQty;

            // Units sold in range
            var soldInRange = await _context.InventoryLedgerEntries
                .Where(e => e.ProductVariantId == variantId
                    && e.Quantity < 0
                    && e.Timestamp >= fromDate
                    && e.Timestamp <= toDate
                    && (e.TransactionType == InventoryTransactionType.SaleConfirmed
                        || e.TransactionType == InventoryTransactionType.BuyerSale))
                .SumAsync(e => (int?)e.Quantity) ?? 0;

            totalCogs += Math.Abs(soldInRange) * avgCostPerUnit;
        }

        // Commissions paid (settled)
        var commissions = await _context.CommissionLedgerEntries
            .Where(e => e.IsSettled && e.CreatedAt >= fromDate && e.CreatedAt <= toDate)
            .SumAsync(e => (decimal?)e.AmountPaid) ?? 0;

        // Delivery earnings paid (settled)
        var delivery = await _context.DeliveryEarnings
            .Where(e => e.IsSettled && e.EarningDate >= fromDate && e.EarningDate <= toDate)
            .SumAsync(e => (decimal?)e.AmountPaid) ?? 0;

        var revenue = guestRevenue + resellerRevenue;
        var grossProfit = revenue - totalCogs;
        var netProfit = grossProfit - commissions - delivery;

        return new FinanceSummaryDto
        {
            From = fromDate,
            To = toDate,
            Revenue = revenue,
            GuestRevenue = guestRevenue,
            ResellerRevenue = resellerRevenue,
            CostOfGoods = totalCogs,
            Commissions = commissions,
            DeliveryEarnings = delivery,
            GrossProfit = grossProfit,
            NetProfit = netProfit,
            OrdersCount = ordersCount,
            SalesCount = salesCount,
        };
    }

    public async Task<List<MonthlyFinanceDto>> GetMonthlyBreakdownAsync(int year)
    {
        var months = new List<MonthlyFinanceDto>();
        for (int m = 1; m <= 12; m++)
        {
            var from = new DateTime(year, m, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1).AddSeconds(-1);

            var summary = await GetSummaryAsync(from, to);
            months.Add(new MonthlyFinanceDto
            {
                Year = year,
                Month = m,
                MonthName = from.ToString("MMMM"),
                Revenue = summary.Revenue,
                CostOfGoods = summary.CostOfGoods,
                Commissions = summary.Commissions,
                DeliveryEarnings = summary.DeliveryEarnings,
                NetProfit = summary.NetProfit,
            });
        }
        return months;
    }

    public async Task<decimal> GetStockValueAsync()
    {
        var variants = await _context.ProductVariants.ToListAsync();
        var total = 0m;

        foreach (var v in variants)
        {
            // Calculate current stock
            var entries = await _context.InventoryLedgerEntries
                .Where(e => e.ProductVariantId == v.Id)
                .ToListAsync();
            var currentQty = entries.Sum(e => e.Quantity);
            if (currentQty <= 0) continue;

            // Use StockCost if available, else avg incoming cost
            var cost = v.StockCost;
            if (cost == null || cost == 0)
            {
                var incoming = entries.Where(e => e.Quantity > 0 && e.TotalCost != null).ToList();
                if (incoming.Any())
                {
                    var totalQty = incoming.Sum(e => e.Quantity);
                    var totalCost = incoming.Sum(e => e.TotalCost!.Value);
                    cost = totalQty > 0 ? totalCost / totalQty : 0;
                }
            }

            total += currentQty * (cost ?? 0);
        }

        return total;
    }
}
