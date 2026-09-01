namespace BakeryApp.Core.Entities;

public class ProductVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string SizeName { get; set; } = string.Empty; // e.g., "5L Bucket", "10L Bucket"
    public decimal UnitPrice { get; set; }
    public decimal BaseCommissionAmount { get; set; } // Per-bucket commission structure
    public bool IsActive { get; set; } = true;
}