using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BakeryApp.Core.Enums;

namespace BakeryApp.Api.Controllers;

public record RecordPayoutRequest(
    Guid ResellerEmployeeId,
    decimal Amount,
    string PaymentReference
);

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,BakeryStaff")]
public class SettlementsController : ControllerBase
{
    private readonly BakeryDbContext _dbContext;

    public SettlementsController(BakeryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("payout")]
    public async Task<IActionResult> RecordPayout([FromBody] RecordPayoutRequest request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Payout amount must be greater than zero.");
        }

        var reseller = await _dbContext.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == request.ResellerEmployeeId && e.RoleType == RoleType.Reseller);

        if (reseller == null)
        {
            return NotFound("Reseller not found.");
        }

        var ledgerEntry = new InventoryLedgerEntry
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.ResellerEmployeeId,
            Quantity = 0,
            Timestamp = DateTime.UtcNow,
            ReferenceNote = $"COMMISSION_PAYOUT:{request.Amount}:{request.PaymentReference}"
        };

        _dbContext.InventoryLedgerEntries.Add(ledgerEntry);
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            Message = "Commission payout recorded successfully.",
            PayoutId = ledgerEntry.Id,
            ResellerId = request.ResellerEmployeeId,
            AmountPaid = request.Amount,
            PaymentReference = request.PaymentReference
        });
    }

    [HttpGet("balance/{resellerEmployeeId:guid}")]
    public async Task<IActionResult> GetSettlementBalance(Guid resellerEmployeeId)
    {
        var reseller = await _dbContext.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId && e.RoleType == RoleType.Reseller);

        if (reseller == null)
        {
            return NotFound("Reseller not found.");
        }

        var entries = await _dbContext.InventoryLedgerEntries
            .Include(i => i.ProductVariant)
            .Where(i => i.EmployeeId == resellerEmployeeId)
            .ToListAsync();

        var totalEarned = entries
            .Where(e => e.TransactionType == TransactionType.DispatchToReseller)
            .Sum(e => e.Quantity * (e.ProductVariant?.BaseCommissionAmount ?? 0m));

        var totalPaid = entries
            .Where(e => e.ReferenceNote != null && e.ReferenceNote.StartsWith("COMMISSION_PAYOUT:"))
            .Sum(e =>
            {
                var parts = e.ReferenceNote!.Split(':');
                return parts.Length > 1 && decimal.TryParse(parts[1], out var amt) ? amt : 0m;
            });

        return Ok(new
        {
            ResellerId = resellerEmployeeId,
            TotalEarnedCommission = totalEarned,
            TotalPaidCommission = totalPaid,
            PendingBalance = totalEarned - totalPaid
        });
    }
}