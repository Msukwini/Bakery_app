using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface IInventoryService
{
    Task<InventoryLedgerEntry> AddStockAsync(Guid productVariantId, int quantity, InventoryTransactionType type, string referenceNote, Guid? employeeId = null, decimal? unitCost = null);
    Task<InventoryLedgerEntry> DeductStockAsync(Guid productVariantId, int quantity, InventoryTransactionType type, string referenceNote, Guid? employeeId = null);
    Task<int> GetCurrentStockAsync(Guid productVariantId);
    Task<List<InventoryLedgerEntry>> GetLedgerAsync(Guid productVariantId, int? limit = null);
}

public class InventoryService : IInventoryService
{
    private readonly BakeryDbContext _context;

    public InventoryService(BakeryDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryLedgerEntry> AddStockAsync(Guid productVariantId, int quantity, InventoryTransactionType type, string referenceNote, Guid? employeeId = null, decimal? unitCost = null)
    {
        if (quantity <= 0) throw new Exception("Quantity must be positive.");
        if (!Enum.IsDefined(typeof(InventoryTransactionType), type))
            throw new Exception("Invalid transaction type.");

        var totalCost = unitCost.HasValue ? unitCost.Value * quantity : (decimal?)null;

        var entry = new InventoryLedgerEntry
        {
            ProductVariantId = productVariantId,
            TransactionType = type,
            Quantity = quantity,
            Timestamp = DateTime.UtcNow,
            ReferenceNote = referenceNote,
            EmployeeId = employeeId,
            UnitCost = unitCost,
            TotalCost = totalCost
        };

        _context.InventoryLedgerEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<InventoryLedgerEntry> DeductStockAsync(Guid productVariantId, int quantity, InventoryTransactionType type, string referenceNote, Guid? employeeId = null)
    {
        if (quantity <= 0) throw new Exception("Quantity must be positive.");

        var currentStock = await GetCurrentStockAsync(productVariantId);
        if (currentStock < quantity)
            throw new Exception($"Insufficient stock. Available: {currentStock}, Requested: {quantity}");

        var entry = new InventoryLedgerEntry
        {
            ProductVariantId = productVariantId,
            TransactionType = type,
            Quantity = -quantity,
            Timestamp = DateTime.UtcNow,
            ReferenceNote = referenceNote,
            EmployeeId = employeeId
        };

        _context.InventoryLedgerEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<int> GetCurrentStockAsync(Guid productVariantId)
    {
        var entries = await _context.InventoryLedgerEntries
            .Where(e => e.ProductVariantId == productVariantId)
            .ToListAsync();

        return entries.Sum(e => e.Quantity);
    }

    public async Task<List<InventoryLedgerEntry>> GetLedgerAsync(Guid productVariantId, int? limit = null)
    {
        var query = _context.InventoryLedgerEntries
            .Where(e => e.ProductVariantId == productVariantId)
            .OrderByDescending(e => e.Timestamp);

        if (limit.HasValue)
            query = (IOrderedQueryable<InventoryLedgerEntry>)query.Take(limit.Value);

        return await query.ToListAsync();
    }
}
