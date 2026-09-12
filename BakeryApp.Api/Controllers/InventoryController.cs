using BakeryApp.Api.DTOs;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly BakeryApp.Infrastructure.Data.BakeryDbContext _context;

    public InventoryController(IInventoryService inventoryService, BakeryApp.Infrastructure.Data.BakeryDbContext context)
    {
        _inventoryService = inventoryService;
        _context = context;
    }

    [HttpPost("add")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddStock([FromBody] AddStockRequest request)
    {
        var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var employeeId = !string.IsNullOrEmpty(employeeIdClaim) ? Guid.Parse(employeeIdClaim) : (Guid?)null;

        try
        {
            var entry = await _inventoryService.AddStockAsync(
                request.ProductVariantId,
                request.Quantity,
                request.TransactionType,
                request.ReferenceNote ?? "",
                employeeId,
                request.UnitCost
            );
            return Ok(new { id = entry.Id, message = "Stock added successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("deduct")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeductStock([FromBody] DeductStockRequest request)
    {
        var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var employeeId = !string.IsNullOrEmpty(employeeIdClaim) ? Guid.Parse(employeeIdClaim) : (Guid?)null;

        try
        {
            var entry = await _inventoryService.DeductStockAsync(
                request.ProductVariantId,
                request.Quantity,
                request.TransactionType,
                request.ReferenceNote ?? "",
                employeeId
            );
            return Ok(new { id = entry.Id, message = "Stock deducted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("stock/{productVariantId}")]
    public async Task<IActionResult> GetStock(Guid productVariantId)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == productVariantId);

        if (variant == null) return NotFound("Product variant not found.");

        var stock = await _inventoryService.GetCurrentStockAsync(productVariantId);

        return Ok(new StockResponse
        {
            ProductVariantId = variant.Id,
            ProductName = variant.Product?.Name ?? "Unknown",
            VariantName = variant.SizeName,
            CurrentStock = stock
        });
    }

    [HttpGet("ledger/{productVariantId}")]
    public async Task<IActionResult> GetLedger(Guid productVariantId, [FromQuery] int? limit = 100)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == productVariantId);

        if (variant == null) return NotFound("Product variant not found.");

        var entries = await _inventoryService.GetLedgerAsync(productVariantId, limit);

        var response = entries.Select(e => new LedgerEntryResponse
        {
            Id = e.Id,
            ProductVariantId = e.ProductVariantId,
            ProductName = variant.Product?.Name ?? "Unknown",
            VariantName = variant.SizeName,
            TransactionType = e.TransactionType,
            Quantity = e.Quantity,
            Timestamp = e.Timestamp,
            ReferenceNote = e.ReferenceNote,
            EmployeeCode = e.Employee?.Code,
            UnitCost = e.UnitCost,
            TotalCost = e.TotalCost
        });

        return Ok(response);
    }
}
