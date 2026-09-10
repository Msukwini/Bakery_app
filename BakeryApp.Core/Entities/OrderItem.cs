namespace BakeryApp.Core.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public BuyerOrder Order { get; set; } = null!;
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Historical price lock
    public decimal TotalPrice => Quantity * UnitPrice;
}
