using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CreateVariantDto> Variants { get; set; } = new();
}

public class CreateVariantDto
{
    public string SizeName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly BakeryDbContext _context;
    private readonly string _uploadDir = "/opt/bakery/uploads/product-pictures";
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public AdminProductsController(BakeryDbContext context)
    {
        _context = context;
        if (!Directory.Exists(_uploadDir))
            Directory.CreateDirectory(_uploadDir);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var products = await _context.Products
            .Include(p => p.Variants)
            .OrderByDescending(p => p.IsActive).ThenBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.IsActive,
                HasImage = !string.IsNullOrEmpty(p.ImagePath),
                Variants = p.Variants
                    .OrderBy(v => v.SizeName)
                    .Select(v => new { v.Id, v.SizeName, v.UnitPrice, v.IsActive })
                    .ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name is required." });

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? "",
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        foreach (var v in dto.Variants)
        {
            if (string.IsNullOrWhiteSpace(v.SizeName) || v.UnitPrice <= 0) continue;
            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = product.Id,
                SizeName = v.SizeName.Trim(),
                UnitPrice = v.UnitPrice,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { id = product.Id, name = product.Name });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Name)) product.Name = dto.Name.Trim();
        if (dto.Description != null) product.Description = dto.Description.Trim();
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Updated." });
    }

    [HttpPost("{id}/variants")]
    public async Task<IActionResult> AddVariant(Guid id, [FromBody] CreateVariantDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.SizeName) || dto.UnitPrice <= 0)
            return BadRequest(new { error = "Size name and unit price are required." });

        var variant = new ProductVariant
        {
            ProductId = id,
            SizeName = dto.SizeName.Trim(),
            UnitPrice = dto.UnitPrice,
            IsActive = true
        };

        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();
        return Ok(new { id = variant.Id });
    }

    [HttpDelete("variants/{variantId}")]
    public async Task<IActionResult> DeactivateVariant(Guid variantId)
    {
        var variant = await _context.ProductVariants.FindAsync(variantId);
        if (variant == null) return NotFound();

        variant.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Variant deactivated." });
    }

    [HttpPost("{id}/picture")]
    public async Task<IActionResult> UploadPicture(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });
        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { error = "File too large (max 5 MB)." });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { error = "Only JPEG, PNG, or WebP images allowed." });

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (!string.IsNullOrEmpty(product.ImagePath))
        {
            var oldPath = Path.Combine(_uploadDir, product.ImagePath);
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";
        var fileName = $"{id}{ext}";
        var fullPath = Path.Combine(_uploadDir, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        product.ImagePath = fileName;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Picture uploaded.", path = fileName });
    }
}

[ApiController]
[Route("api/products")]
public class PublicProductPictureController : ControllerBase
{
    private readonly BakeryDbContext _context;
    private readonly string _uploadDir = "/opt/bakery/uploads/product-pictures";

    public PublicProductPictureController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet("picture/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPicture(Guid productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || string.IsNullOrEmpty(product.ImagePath))
            return NotFound();

        var fullPath = Path.Combine(_uploadDir, product.ImagePath);
        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var ext = Path.GetExtension(fullPath).ToLower();
        var contentType = ext switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        Response.Headers.Append("Cache-Control", "public, max-age=3600");
        return File(bytes, contentType);
    }
}
