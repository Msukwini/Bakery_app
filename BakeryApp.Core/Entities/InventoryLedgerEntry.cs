namespace BakeryApp.Core.Entities;

using BakeryApp.Core.Enums;

public class InventoryLedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public InventoryTransactionType TransactionType { get; set; }
    public int Quantity { get; set; } // Positive for incoming stock, negative for allocations/sales
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ReferenceNote { get; set; } = string.Empty;
    
    public Guid? EmployeeId { get; set; }
    public EmployeeId? Employee { get; set; }
}