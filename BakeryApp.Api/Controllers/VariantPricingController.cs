using BakeryApp.Api.DTOs;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/admin/variants")]
[Authorize(Roles = "Admin")]
public class VariantPricingController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public VariantPricingController(BakeryDbContext context)
    {
        _context = context;
    }

    /// <summary>Update pricing for a single variant.</summary>
    [HttpPut("{variantId}/pricing")]
    public async Task<IActionResult> UpdatePricing(Guid variantId, [FromBody] UpdateVariantPricingDto dto)
    {
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null) return NotFound();

        if (dto.UnitPrice.HasValue) variant.UnitPrice = dto.UnitPrice.Value;
        if (dto.GuestPrice.HasValue) variant.GuestPrice = dto.GuestPrice.Value;
        if (dto.StockCost.HasValue) variant.StockCost = dto.StockCost.Value;
        if (dto.IsActive.HasValue) variant.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Pricing updated." });
    }

    /// <summary>List all variants with pricing for admin.</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var variants = await _context.ProductVariants
            .Include(v => v.Product)
            .OrderBy(v => v.Product.Name).ThenBy(v => v.SizeName)
            .Select(v => new
            {
                v.Id,
                v.ProductId,
                ProductName = v.Product.Name,
                v.SizeName,
                v.UnitPrice,
                v.GuestPrice,
                v.StockCost,
                v.IsActive
            })
            .ToListAsync();

        return Ok(variants);
    }
}
