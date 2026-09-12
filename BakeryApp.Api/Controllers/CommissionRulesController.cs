using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

public class CreateCommissionRuleDto
{
    public Guid ProductVariantId { get; set; }
    public decimal RatePerUnit { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class UpdateCommissionRuleDto
{
    public decimal RatePerUnit { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Notes { get; set; }
}

[ApiController]
[Route("api/admin/commission-rules")]
[Authorize(Roles = "Admin")]
public class CommissionRulesController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public CommissionRulesController(BakeryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all commission rules grouped by product variant, with active + history.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var variants = await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.IsActive)
            .OrderBy(v => v.Product.Name).ThenBy(v => v.SizeName)
            .ToListAsync();

        var rules = await _context.CommissionRules
            .OrderByDescending(r => r.EffectiveDate)
            .ToListAsync();

        var result = variants.Select(v =>
        {
            var variantRules = rules.Where(r => r.ProductVariantId == v.Id).ToList();
            var active = variantRules.FirstOrDefault(r => r.IsActive
                && r.EffectiveDate <= DateTime.UtcNow
                && (r.EndDate == null || r.EndDate >= DateTime.UtcNow));

            return new
            {
                variantId = v.Id,
                productName = v.Product?.Name ?? "Unknown",
                variantName = v.SizeName,
                activeRule = active == null ? null : new
                {
                    active.Id,
                    active.RatePerUnit,
                    active.EffectiveDate,
                    active.EndDate
                },
                history = variantRules.Select(r => new
                {
                    r.Id,
                    r.RatePerUnit,
                    r.EffectiveDate,
                    r.EndDate,
                    r.IsActive,
                    IsCurrent = r.Id == active?.Id
                }).ToList()
            };
        });

        return Ok(result);
    }

    /// <summary>
    /// Create a new commission rule. If there's an active rule, it gets ended the day before this one starts.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommissionRuleDto dto)
    {
        if (dto.RatePerUnit < 0)
            return BadRequest(new { error = "Rate must be positive." });

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == dto.ProductVariantId);
        if (variant == null)
            return BadRequest(new { error = "Product variant not found." });

        // End any currently-active rule the day before this new rule starts
        var currentActive = await _context.CommissionRules
            .Where(r => r.ProductVariantId == dto.ProductVariantId
                     && r.IsActive
                     && r.EndDate == null)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

        if (currentActive != null)
        {
            currentActive.EndDate = dto.EffectiveDate.AddDays(-1);
        }

        var rule = new CommissionRule
        {
            ProductVariantId = dto.ProductVariantId,
            RatePerUnit = dto.RatePerUnit,
            EffectiveDate = dto.EffectiveDate,
            IsActive = true
        };

        _context.CommissionRules.Add(rule);
        await _context.SaveChangesAsync();

        return Ok(new { id = rule.Id, message = "Commission rule created." });
    }

    /// <summary>
    /// Manually update a rule's rate. Only allowed for the active rule.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommissionRuleDto dto)
    {
        var rule = await _context.CommissionRules.FindAsync(id);
        if (rule == null) return NotFound();

        if (dto.RatePerUnit < 0)
            return BadRequest(new { error = "Rate must be positive." });

        // If this is the active rule, allow direct edit
        var isActive = rule.IsActive
            && rule.EffectiveDate <= DateTime.UtcNow
            && (rule.EndDate == null || rule.EndDate >= DateTime.UtcNow);

        if (!isActive)
            return BadRequest(new { error = "Only active rules can be edited. Create a new rule instead." });

        rule.RatePerUnit = dto.RatePerUnit;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Commission rule updated." });
    }

    /// <summary>
    /// Deactivate a rule (soft delete — history preserved).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var rule = await _context.CommissionRules.FindAsync(id);
        if (rule == null) return NotFound();

        rule.IsActive = false;
        if (rule.EndDate == null) rule.EndDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Commission rule deactivated." });
    }
}
