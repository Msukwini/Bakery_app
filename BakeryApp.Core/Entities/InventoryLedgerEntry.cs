namespace BakeryApp.Core.Entities;

using BakeryApp.Core.Enums;

public class InventoryLedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public InventoryTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ReferenceNote { get; set; } = string.Empty;

    /// <summary>Unit cost for this movement (production/purchase). Null for outbound.</summary>
    public decimal? UnitCost { get; set; }

    /// <summary>Total cost = Quantity × UnitCost</summary>
    public decimal? TotalCost { get; set; }

    public Guid? EmployeeId { get; set; }
    public EmployeeId? Employee { get; set; }
}
