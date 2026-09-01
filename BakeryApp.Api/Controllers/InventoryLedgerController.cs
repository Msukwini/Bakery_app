using BakeryApp.Api.DTOs;
using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryLedgerController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public InventoryLedgerController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryLedgerResponseDto>>> GetLedgerEntries(
        [FromQuery] Guid? productVariantId,
        [FromQuery] Guid? employeeId)
    {
        var query = _context.InventoryLedgerEntries
            .Include(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
            .Include(i => i.Employee)
                .ThenInclude(e => e!.Person)
            .AsQueryable();

        if (productVariantId.HasValue)
        {
            query = query.Where(i => i.ProductVariantId == productVariantId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(i => i.EmployeeId == employeeId.Value);
        }

        var entries = await query
            .OrderByDescending(i => i.Timestamp)
            .Select(i => new InventoryLedgerResponseDto(
                i.Id,
                i.ProductVariantId,
                i.ProductVariant.Product.Name,
                i.ProductVariant.SizeName,
                i.TransactionType,
                i.TransactionType.ToString(),
                i.Quantity,
                i.Timestamp,
                i.ReferenceNote,
                i.EmployeeId,
                i.Employee != null ? i.Employee.Code : null,
                i.Employee != null ? $"{i.Employee.Person.FirstName} {i.Employee.Person.LastName}" : null
            ))
            .ToListAsync();

        return Ok(entries);
    }

    [HttpGet("balance/{productVariantId:guid}")]
    public async Task<ActionResult<StockBalanceDto>> GetStockBalance(Guid productVariantId)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == productVariantId);

        if (variant == null) return NotFound("Product variant not found.");

        var totalStock = await _context.InventoryLedgerEntries
            .Where(i => i.ProductVariantId == productVariantId)
            .SumAsync(i => i.Quantity);

        return Ok(new StockBalanceDto(
            variant.Id,
            variant.Product.Name,
            variant.SizeName,
            totalStock
        ));
    }

    [HttpPost("production")]
    public async Task<ActionResult<InventoryLedgerResponseDto>> RecordProductionBatch(CreateProductionBatchDto dto)
    {
        if (dto.Quantity <= 0) return BadRequest("Quantity must be greater than zero.");

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId);

        if (variant == null) return BadRequest("Product variant does not exist.");

        var entry = new InventoryLedgerEntry
        {
            ProductVariantId = dto.ProductVariantId,
            TransactionType = InventoryTransactionType.ProductionBatch,
            Quantity = dto.Quantity,
            ReferenceNote = string.IsNullOrWhiteSpace(dto.ReferenceNote) ? "Production Batch Added" : dto.ReferenceNote,
            Timestamp = DateTime.UtcNow
        };

        _context.InventoryLedgerEntries.Add(entry);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLedgerEntries), new { productVariantId = entry.ProductVariantId }, MapToDto(entry, variant));
    }

    [HttpPost("allocation")]
    public async Task<ActionResult<InventoryLedgerResponseDto>> RecordAllocation(CreateAllocationDto dto)
    {
        if (dto.Quantity <= 0) return BadRequest("Quantity must be greater than zero.");

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId);

        if (variant == null) return BadRequest("Product variant does not exist.");

        var reseller = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == dto.ResellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller);

        if (reseller == null) return BadRequest("Invalid reseller employee ID.");

        var currentStock = await _context.InventoryLedgerEntries
            .Where(i => i.ProductVariantId == dto.ProductVariantId)
            .SumAsync(i => i.Quantity);

        if (currentStock < dto.Quantity)
        {
            return BadRequest($"Insufficient stock. Available: {currentStock}, Requested: {dto.Quantity}");
        }

        var entry = new InventoryLedgerEntry
        {
            ProductVariantId = dto.ProductVariantId,
            EmployeeId = dto.ResellerEmployeeId,
            TransactionType = InventoryTransactionType.AllocatedToReseller,
            Quantity = -dto.Quantity,
            ReferenceNote = string.IsNullOrWhiteSpace(dto.ReferenceNote) ? $"Allocated to {reseller.Code}" : dto.ReferenceNote,
            Timestamp = DateTime.UtcNow
        };

        _context.InventoryLedgerEntries.Add(entry);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLedgerEntries), new { productVariantId = entry.ProductVariantId }, MapToDto(entry, variant, reseller));
    }

    [HttpPost("adjustment")]
    public async Task<ActionResult<InventoryLedgerResponseDto>> RecordAdjustment(CreateAdjustmentDto dto)
    {
        if (dto.Quantity == 0) return BadRequest("Adjustment quantity cannot be zero.");

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId);

        if (variant == null) return BadRequest("Product variant does not exist.");

        var entry = new InventoryLedgerEntry
        {
            ProductVariantId = dto.ProductVariantId,
            TransactionType = dto.TransactionType,
            Quantity = dto.Quantity,
            ReferenceNote = dto.ReferenceNote,
            Timestamp = DateTime.UtcNow
        };

        _context.InventoryLedgerEntries.Add(entry);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLedgerEntries), new { productVariantId = entry.ProductVariantId }, MapToDto(entry, variant));
    }

    private static InventoryLedgerResponseDto MapToDto(InventoryLedgerEntry entry, ProductVariant variant, EmployeeId? employee = null)
    {
        return new InventoryLedgerResponseDto(
            entry.Id,
            entry.ProductVariantId,
            variant.Product.Name,
            variant.SizeName,
            entry.TransactionType,
            entry.TransactionType.ToString(),
            entry.Quantity,
            entry.Timestamp,
            entry.ReferenceNote,
            entry.EmployeeId,
            employee?.Code,
            employee != null ? $"{employee.Person.FirstName} {employee.Person.LastName}" : null
        );
    }
}