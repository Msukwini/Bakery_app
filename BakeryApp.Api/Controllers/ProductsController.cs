using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public ProductsController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet("variants")]
    public async Task<IActionResult> ListVariants()
    {
        var variants = await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.IsActive && v.Product.IsActive)
            .OrderBy(v => v.Product.Name).ThenBy(v => v.SizeName)
            .Select(v => new
            {
                v.Id,
                ProductName = v.Product.Name,
                v.SizeName,
                UnitPrice = v.GuestPrice ?? v.UnitPrice,
                ResellerPrice = v.UnitPrice,
                ProductDescription = v.Product.Description
            })
            .ToListAsync();

        return Ok(variants);
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var products = await _context.Products
            .Include(p => p.Variants)
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                Variants = p.Variants
                    .Where(v => v.IsActive)
                    .Select(v => new
                    {
                        v.Id,
                        v.SizeName,
                        UnitPrice = v.GuestPrice ?? v.UnitPrice
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(products);
    }
}
