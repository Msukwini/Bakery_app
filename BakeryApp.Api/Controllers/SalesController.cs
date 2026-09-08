using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BakeryApp.Core.Enums;

namespace BakeryApp.Api.Controllers;

public record DispatchOrderRequest(
    Guid ResellerEmployeeId,
    Guid ProductVariantId,
    int Quantity
);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly BakeryDbContext _dbContext;

    public SalesController(BakeryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("dispatch")]
    [Authorize(Roles = "Admin,BakeryStaff")]
    public async Task<IActionResult> DispatchOrder([FromBody] DispatchOrderRequest request)
    {
        var reseller = await _dbContext.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == request.ResellerEmployeeId && e.RoleType == RoleType.Reseller);

        if (reseller == null)
        {
            return NotFound("Reseller not found.");
        }

        var variant = await _dbContext.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

        if (variant == null)
        {
            return NotFound("Product variant not found.");
        }

        var ledgerEntry = new InventoryLedgerEntry
        {
            Id = Guid.NewGuid(),
            ProductVariantId = request.ProductVariantId,
            EmployeeId = request.ResellerEmployeeId,
            TransactionType = TransactionType.DispatchToReseller,
            Quantity = request.Quantity,
            Timestamp = DateTime.UtcNow,
            ReferenceNote = $"Dispatched {request.Quantity} units to reseller {reseller.Code}"
        };

        _dbContext.InventoryLedgerEntries.Add(ledgerEntry);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Order dispatched successfully.", LedgerEntryId = ledgerEntry.Id });
    }

    [HttpGet("commissions/{resellerEmployeeId:guid}")]
    [Authorize(Roles = "Admin,Reseller")]
    public async Task<IActionResult> GetCommissions(Guid resellerEmployeeId)
    {
        var reseller = await _dbContext.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId);

        if (reseller == null)
        {
            return NotFound("Reseller not found.");
        }

        var entries = await _dbContext.InventoryLedgerEntries
            .Include(i => i.ProductVariant)
            .Where(i => i.EmployeeId == resellerEmployeeId && i.TransactionType == TransactionType.DispatchToReseller)
            .ToListAsync();

        var totalCommission = entries.Sum(e => e.Quantity * (e.ProductVariant?.BaseCommissionAmount ?? 0m));

        return Ok(new
        {
            ResellerId = resellerEmployeeId,
            TotalDispatched = entries.Sum(e => e.Quantity),
            TotalCommissionEarned = totalCommission
        });
    }
}