namespace BakeryApp.Core.Entities;

public class ProductVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string SizeName { get; set; } = string.Empty;

    /// <summary>Default selling price used for both guest orders and reseller sales</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>What direct customers pay. Falls back to UnitPrice if null.</summary>
    public decimal? GuestPrice { get; set; }

    /// <summary>What the bakery pays to produce/purchase one unit. Used for profit calc.</summary>
    public decimal? StockCost { get; set; }

    public decimal BaseCommissionAmount { get; set; }
    public bool IsActive { get; set; } = true;
}
