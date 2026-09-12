using BakeryApp.Core.Enums;

namespace BakeryApp.Core.Entities;

public class MilestoneConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    /// <summary>Number of units (orders) a reseller must sell to trigger the bonus.</summary>
    public int TargetUnits { get; set; }

    /// <summary>Bonus amount to alert admin about (separate from per-unit commission).</summary>
    public decimal BonusAmount { get; set; }

    public MilestoneTimeframe Timeframe { get; set; } = MilestoneTimeframe.Monthly;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedByAdminId { get; set; }
}
