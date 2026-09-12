namespace BakeryApp.Core.Entities;

public class MilestoneAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MilestoneConfigId { get; set; }
    public MilestoneConfig MilestoneConfig { get; set; } = null!;

    public Guid ResellerEmployeeId { get; set; }
    public EmployeeId Reseller { get; set; } = null!;

    public int AchievedUnits { get; set; }
    public decimal BonusOwed { get; set; }

    /// <summary>e.g., "2026-09" for monthly, "2026-W37" for weekly, "LIFETIME"</summary>
    public string PeriodKey { get; set; } = string.Empty;

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
    public string? AdminNotes { get; set; }
}
