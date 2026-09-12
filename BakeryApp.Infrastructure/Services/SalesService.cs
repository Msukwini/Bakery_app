using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface ISalesService
{
    Task<ResellerSale> RecordSaleAsync(Guid resellerEmployeeId, Guid productVariantId, int quantity, decimal unitPriceAtSale, Guid? employeeId = null);
    Task<List<CommissionLedgerEntry>> GetCommissionLedgerAsync(Guid resellerEmployeeId);
    Task<decimal> GetOutstandingCommissionAsync(Guid resellerEmployeeId);
}

public class SalesService : ISalesService
{
    private readonly BakeryDbContext _context;
    private readonly IInventoryService _inventoryService;

    private readonly IMilestoneService _milestoneService;

    public SalesService(BakeryDbContext context, IInventoryService inventoryService, IMilestoneService milestoneService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _milestoneService = milestoneService;
    }

    public async Task<ResellerSale> RecordSaleAsync(Guid resellerEmployeeId, Guid productVariantId, int quantity, decimal unitPriceAtSale, Guid? employeeId = null)
    {
        if (quantity <= 0) throw new Exception("Quantity must be positive.");

        var reseller = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller && e.IsActive);
        if (reseller == null) throw new Exception("Reseller not found or inactive.");

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == productVariantId);
        if (variant == null) throw new Exception("Product variant not found.");

        var rule = await _context.CommissionRules
            .Where(r => r.ProductVariantId == productVariantId && r.IsActive && r.EffectiveDate <= DateTime.UtcNow)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();
        if (rule == null) throw new Exception("No active commission rule found for this product.");

        var commissionPerUnit = rule.RatePerUnit;

        var sale = new ResellerSale
        {
            ResellerEmployeeId = resellerEmployeeId,
            Reseller = reseller,
            ProductVariantId = productVariantId,
            ProductVariant = variant,
            Quantity = quantity,
            UnitPriceAtSale = unitPriceAtSale,
            SaleDate = DateTime.UtcNow,
            CommissionRuleId = rule.Id,
            CommissionRule = rule
        };

        _context.ResellerSales.Add(sale);
        await _context.SaveChangesAsync();

        var ledgerEntries = new List<CommissionLedgerEntry>();
        for (int i = 0; i < quantity; i++)
        {
            ledgerEntries.Add(new CommissionLedgerEntry
            {
                ResellerEmployeeId = resellerEmployeeId,
                Reseller = reseller,
                ResellerSaleId = sale.Id,
                AmountEarned = commissionPerUnit,
                AmountPaid = 0,
                IsSettled = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.CommissionLedgerEntries.AddRange(ledgerEntries);
        await _context.SaveChangesAsync();

        await _inventoryService.DeductStockAsync(
            productVariantId,
            quantity,
            InventoryTransactionType.SaleConfirmed,
            $"Sale by {reseller.Code} - Sale ID: {sale.Id}",
            employeeId
        );

        // Check milestone triggers
        await _milestoneService.CheckAndTriggerAsync(resellerEmployeeId, productVariantId);

        return sale;
    }

    public async Task<List<CommissionLedgerEntry>> GetCommissionLedgerAsync(Guid resellerEmployeeId)
    {
        return await _context.CommissionLedgerEntries
            .Where(e => e.ResellerEmployeeId == resellerEmployeeId)
            .Include(e => e.ResellerSale)
                .ThenInclude(s => s.ProductVariant)
                    .ThenInclude(v => v.Product)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetOutstandingCommissionAsync(Guid resellerEmployeeId)
    {
        var entries = await _context.CommissionLedgerEntries
            .Where(e => e.ResellerEmployeeId == resellerEmployeeId && !e.IsSettled)
            .ToListAsync();
        return entries.Sum(e => e.AmountEarned - e.AmountPaid);
    }
}
