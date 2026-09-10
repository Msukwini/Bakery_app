using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface ISettlementService
{
    Task<List<CommissionLedgerEntry>> ProcessPayoutAsync(Guid resellerEmployeeId, decimal amount, string? notes = null);
    Task<decimal> GetOutstandingBalanceAsync(Guid resellerEmployeeId);
    Task<decimal> GetTotalEarnedAsync(Guid resellerEmployeeId);
    Task<decimal> GetTotalPaidAsync(Guid resellerEmployeeId);
}

public class SettlementService : ISettlementService
{
    private readonly BakeryDbContext _context;

    public SettlementService(BakeryDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommissionLedgerEntry>> ProcessPayoutAsync(Guid resellerEmployeeId, decimal amount, string? notes = null)
    {
        if (amount <= 0) throw new Exception("Payout amount must be positive.");

        var reseller = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller && e.IsActive);
        if (reseller == null) throw new Exception("Reseller not found or inactive.");

        var unsettledEntries = await _context.CommissionLedgerEntries
            .Where(e => e.ResellerEmployeeId == resellerEmployeeId && !e.IsSettled)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        if (!unsettledEntries.Any())
            throw new Exception("No outstanding commission to pay.");

        var totalOutstanding = unsettledEntries.Sum(e => e.AmountEarned - e.AmountPaid);
        if (amount > totalOutstanding)
            throw new Exception($"Amount exceeds outstanding balance. Outstanding: {totalOutstanding}, Requested: {amount}");

        var remaining = amount;
        var updatedEntries = new List<CommissionLedgerEntry>();

        foreach (var entry in unsettledEntries)
        {
            if (remaining <= 0) break;

            var due = entry.AmountEarned - entry.AmountPaid;
            if (due <= 0) continue;

            if (remaining >= due)
            {
                entry.AmountPaid += due;
                entry.IsSettled = true;
                remaining -= due;
            }
            else
            {
                entry.AmountPaid += remaining;
                remaining = 0;
            }

            updatedEntries.Add(entry);
        }

        await _context.SaveChangesAsync();

        var auditLog = new AuditLog
        {
            Action = "COMMISSION_PAYOUT",
            EntityType = "CommissionLedgerEntry",
            EntityId = resellerEmployeeId.ToString(),
            NewValue = $"Paid {amount} to {reseller.Code}. Notes: {notes ?? "N/A"}",
            Timestamp = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return updatedEntries;
    }

    public async Task<decimal> GetOutstandingBalanceAsync(Guid resellerEmployeeId)
    {
        var entries = await _context.CommissionLedgerEntries
            .Where(e => e.ResellerEmployeeId == resellerEmployeeId)
            .ToListAsync();
        return entries.Sum(e => e.AmountEarned - e.AmountPaid);
    }

    public async Task<decimal> GetTotalEarnedAsync(Guid resellerEmployeeId)
    {
        var entries = await _context.CommissionLedgerEntries
            .Where(e => e.ResellerEmployeeId == resellerEmployeeId)
            .ToListAsync();
        return entries.Sum(e => e.AmountEarned);
    }

    public async Task<decimal> GetTotalPaidAsync(Guid resellerEmployeeId)
    {
        var entries = await _context.CommissionLedgerEntries
            .Where(e => e.ResellerEmployeeId == resellerEmployeeId)
            .ToListAsync();
        return entries.Sum(e => e.AmountPaid);
    }
}
