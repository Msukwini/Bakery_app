using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public SalesController(BakeryDbContext context)
    {
        _context = context;
    }

    // POST: api/sales/dispatch
    [HttpPost("dispatch")]
    public async Task<IActionResult> DispatchToReseller([FromBody] DispatchOrderRequest request)
    {
        var reseller = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == request.ResellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller);

        if (reseller == null)
            return NotFound("Reseller employee not found or inactive.");

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

        if (variant == null)
            return NotFound("Product variant not found.");

        // Check available stock
        var availableStock = await _context.InventoryLedgerEntries
            .Where(e => e.ProductVariantId == request.ProductVariantId)
            .SumAsync(e => e.Quantity);

        if (availableStock < request.Quantity)
            return BadRequest($"Insufficient stock. Available: {availableStock}, Requested: {request.Quantity}");

        // 1. Record stock dispatch (negative quantity entry)
        var stockDeduction = new InventoryLedgerEntry
        {
            ProductVariantId = request.ProductVariantId,
            EmployeeId = reseller.Id,
            TransactionType = InventoryTransactionType.AllocatedToReseller,
            Quantity = -request.Quantity,
            ReferenceNote = $"Dispatched to {reseller.Person.FirstName} {reseller.Person.LastName} ({reseller.Code})",
            Timestamp = DateTime.UtcNow
        };

        _context.InventoryLedgerEntries.Add(stockDeduction);

        await _context.SaveChangesAsync();

        // 2. Calculate financial totals
        var totalSalesValue = variant.UnitPrice * request.Quantity;
        var totalCommissionEarned = variant.BaseCommissionAmount * request.Quantity;

        return Ok(new
        {
            Message = "Order dispatched successfully.",
            ResellerCode = reseller.Code,
            ResellerName = $"{reseller.Person.FirstName} {reseller.Person.LastName}",
            Product = $"{variant.Product.Name} - {variant.SizeName}",
            QuantityDispatched = request.Quantity,
            TotalSalesValue = totalSalesValue,
            CommissionEarned = totalCommissionEarned
        });
    }

    // GET: api/sales/commissions/{resellerEmployeeId}
    [HttpGet("commissions/{resellerEmployeeId}")]
    public async Task<IActionResult> GetResellerCommissions(Guid resellerEmployeeId)
    {
        var reseller = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId);

        if (reseller == null)
            return NotFound("Reseller not found.");

        var dispatches = await _context.InventoryLedgerEntries
            .Include(e => e.ProductVariant)
            .Where(e => e.EmployeeId == resellerEmployeeId && e.TransactionType == InventoryTransactionType.AllocatedToReseller)
            .ToListAsync();

        var totalItemsDispatched = Math.Abs(dispatches.Sum(d => d.Quantity));
        var totalCommissionEarned = dispatches.Sum(d => Math.Abs(d.Quantity) * d.ProductVariant.BaseCommissionAmount);

        return Ok(new
        {
            ResellerCode = reseller.Code,
            ResellerName = $"{reseller.Person.FirstName} {reseller.Person.LastName}",
            TotalItemsDispatched = totalItemsDispatched,
            TotalCommissionEarned = totalCommissionEarned,
            DispatchHistory = dispatches.Select(d => new
            {
                d.Timestamp,
                Size = d.ProductVariant.SizeName,
                Quantity = Math.Abs(d.Quantity),
                CommissionEarned = Math.Abs(d.Quantity) * d.ProductVariant.BaseCommissionAmount
            })
        });
    }
}

public record DispatchOrderRequest(Guid ResellerEmployeeId, Guid ProductVariantId, int Quantity);