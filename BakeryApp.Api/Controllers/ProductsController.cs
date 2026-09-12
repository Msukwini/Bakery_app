using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
                v.UnitPrice
            })
            .ToListAsync();

        return Ok(variants);
    }
}
