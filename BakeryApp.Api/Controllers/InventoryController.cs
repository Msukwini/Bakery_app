using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

public record RecordBatchRequest(
    Guid ProductVariantId,
    int Quantity,
    string? BatchNumber
);

public record WriteOffRequest(
    Guid ProductVariantId,
    int Quantity,
    string Reason
);

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,BakeryStaff")]
public class InventoryController : ControllerBase
{
    private readonly BakeryDbContext _dbContext;

    public InventoryController(BakeryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("batch")]
    public async Task<IActionResult> RecordBatch([FromBody] RecordBatchRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Batch quantity must be greater than zero.");
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
            Quantity = request.Quantity,
            Timestamp = DateTime.UtcNow,
            ReferenceNote = $"Production Batch: {request.BatchNumber ?? "N/A"}"
        };

        _dbContext.InventoryLedgerEntries.Add(ledgerEntry);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Production batch recorded successfully.", LedgerEntryId = ledgerEntry.Id });
    }

    [HttpPost("write-off")]
    public async Task<IActionResult> WriteOffStock([FromBody] WriteOffRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Write-off quantity must be greater than zero.");
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
            Quantity = -request.Quantity,
            Timestamp = DateTime.UtcNow,
            ReferenceNote = $"Write-Off Reason: {request.Reason}"
        };

        _dbContext.InventoryLedgerEntries.Add(ledgerEntry);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Message = "Stock write-off recorded successfully.", LedgerEntryId = ledgerEntry.Id });
    }

    [HttpGet("stock/{productVariantId:guid}")]
    public async Task<IActionResult> GetStockLevel(Guid productVariantId)
    {
        var variant = await _dbContext.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == productVariantId);

        if (variant == null)
        {
            return NotFound("Product variant not found.");
        }

        var totalStock = await _dbContext.InventoryLedgerEntries
            .Where(e => e.ProductVariantId == productVariantId)
            .SumAsync(e => e.Quantity);

        return Ok(new
        {
            ProductVariantId = productVariantId,
            CurrentStockLevel = totalStock
        });
    }
}