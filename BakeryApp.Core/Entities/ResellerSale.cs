namespace BakeryApp.Core.Entities;

public class ResellerSale
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResellerEmployeeId { get; set; } // Must be an EmployeeId with RoleType = Reseller
    public EmployeeId Reseller { get; set; } = null!;
    
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    
    public int Quantity { get; set; }
    public decimal UnitPriceAtSale { get; set; } // Historical price lock
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    
    // Link to the commission rule that was active at the time of sale
    public Guid CommissionRuleId { get; set; }
    public CommissionRule CommissionRule { get; set; } = null!;
}
