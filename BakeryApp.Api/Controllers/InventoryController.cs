using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BakeryApp.Core;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public InventoryController(BakeryDbContext context)
    {
        _context = context;
    }

    // GET: api/inventory/stock
    [HttpGet("stock")]
    public async Task<IActionResult> GetStockLevels()
    {
        var stock = await _context.ProductVariants
            .Include(v => v.Product)
            .Select(v => new
            {
                VariantId = v.Id,
                ProductName = v.Product.Name,
                v.SizeName,
                v.UnitPrice,
                CurrentStock = _context.InventoryLedgerEntries
                    .Where(e => e.ProductVariantId == v.Id)
                    .Sum(e => (int?)e.Quantity) ?? 0
            })
            .ToListAsync();

        return Ok(stock);
    }

    // POST: api/inventory/batch (Supports both routes used in tests/clients)
    [HttpPost("batch")]
    [HttpPost("production-batch")]
    public async Task<IActionResult> RecordProductionBatch([FromBody] ProductionBatchRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        if (request.Quantity <= 0)
        {
            return BadRequest("Production quantity must be greater than zero.");
        }

        var variant = await _context.ProductVariants.FindAsync(request.ProductVariantId);
        if (variant == null)
        {
            return NotFound("Product variant not found.");
        }

        var entry = new InventoryLedgerEntry
        {
            Id = Guid.NewGuid(),
            ProductVariantId = request.ProductVariantId,
            Quantity = request.Quantity,
            TransactionType = InventoryTransactionType.ProductionBatch,
            Timestamp = DateTime.UtcNow,
            ReferenceNote = request.ReferenceNote ?? "Manufactured stock production batch",
            EmployeeId = request.EmployeeId
        };

        _context.InventoryLedgerEntries.Add(entry);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Production batch recorded successfully", entryId = entry.Id, quantityAdded = request.Quantity });
    }
}

public record ProductionBatchRequest(Guid ProductVariantId, int Quantity, string? ReferenceNote, Guid? EmployeeId);