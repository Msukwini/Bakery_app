using BakeryApp.Core.Enums;

namespace BakeryApp.Core.Entities;

public class BuyerOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty; // BUY-XXXX
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime RequiredDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? AdminNotes { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
