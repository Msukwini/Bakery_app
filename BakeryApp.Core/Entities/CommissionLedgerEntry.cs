namespace BakeryApp.Core.Entities;

public class CommissionLedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResellerEmployeeId { get; set; }
    public EmployeeId Reseller { get; set; } = null!;
    
    public Guid ResellerSaleId { get; set; }
    public ResellerSale ResellerSale { get; set; } = null!;
    
    public decimal AmountEarned { get; set; } // Positive (e.g., +30)
    public decimal AmountPaid { get; set; } // Usually 0 until paid
    public bool IsSettled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
