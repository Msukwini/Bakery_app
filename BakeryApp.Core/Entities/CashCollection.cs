using BakeryApp.Core.Enums;

namespace BakeryApp.Core.Entities;

public class CashCollection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DeliveryEmployeeId { get; set; }
    public EmployeeId DeliveryEmployee { get; set; } = null!;

    public DateTime CollectionDate { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal Variance { get; set; }

    public VarianceReason VarianceReason { get; set; } = VarianceReason.NONE;
    public string? VarianceNotes { get; set; }

    public CollectionStatus Status { get; set; } = CollectionStatus.RECORDED;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReconciledAt { get; set; }
    public Guid? ReconciledByAdminId { get; set; }
    public EmployeeId? ReconciledByAdmin { get; set; }
    public string? AdminNotes { get; set; }
}
