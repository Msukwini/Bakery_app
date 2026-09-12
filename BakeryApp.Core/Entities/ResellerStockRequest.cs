using BakeryApp.Core.Enums;

namespace BakeryApp.Core.Entities;

public class ResellerStockRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ResellerEmployeeId { get; set; }
    public EmployeeId Reseller { get; set; } = null!;

    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public int RequestedQuantity { get; set; }
    public int? AllocatedQuantity { get; set; }

    public StockRequestStatus Status { get; set; } = StockRequestStatus.PENDING;

    // Timeline
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public EmployeeId? ReviewedByAdmin { get; set; }
    public DateTime? AllocatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Delivery tracking
    public Guid? AssignedDeliveryEmployeeId { get; set; }
    public EmployeeId? AssignedDeliveryEmployee { get; set; }
    public Guid? ActualDeliveryEmployeeId { get; set; }
    public EmployeeId? ActualDeliveryEmployee { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DeliveryCompletedAt { get; set; }
    public int? DeliveredQuantity { get; set; }
    public string? DeliveryNotes { get; set; }

    // Receipt confirmation
    public int? ReceivedQuantity { get; set; }
    public string? ReceiptNotes { get; set; }

    // Variance handling
    public int? Variance { get; set; }
    public VarianceStatus VarianceStatus { get; set; } = VarianceStatus.NONE;
    public string? VarianceNotes { get; set; }
    public Guid? VarianceResolvedByAdminId { get; set; }
    public DateTime? VarianceResolvedAt { get; set; }

    // Legacy notes
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? ResellerNotes { get; set; }

    public Guid? InventoryLedgerEntryId { get; set; }
}
