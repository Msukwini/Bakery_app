namespace BakeryApp.Core.Entities;

public class CommissionRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    
    public decimal RatePerUnit { get; set; } // e.g., 30 for R30 per bucket
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; } // Null means currently active
    public bool IsActive { get; set; } = true;
    public string? TierConfigurationJson { get; set; } // For future tiered commissions
}
