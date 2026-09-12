using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface IOrderService
{
    Task<BuyerOrder> CreateOrderAsync(string? customerName, string? customerPhone, string? customerEmail, string? deliveryAddress, DateTime requiredDate, List<(Guid ProductVariantId, int Quantity)> items, string? paymentMethod = null);
    Task<BuyerOrder> GetOrderAsync(Guid orderId);
    Task<List<BuyerOrder>> GetOrdersAsync(OrderStatus? status = null);
    Task<BuyerOrder> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, string? adminNotes = null);
    Task<BuyerOrder> UpdatePaymentAsync(Guid orderId, string paymentReference, bool isPaid);
}

public class OrderService : IOrderService
{
    private readonly BakeryDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;

    public OrderService(BakeryDbContext context, IInventoryService inventoryService, INotificationService notificationService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
    }

    public async Task<BuyerOrder> CreateOrderAsync(string? customerName, string? customerPhone, string? customerEmail, string? deliveryAddress, DateTime requiredDate, List<(Guid ProductVariantId, int Quantity)> items, string? paymentMethod = null)
    {
        if (!items.Any()) throw new Exception("Order must contain at least one item.");

        var count = await _context.BuyerOrders.CountAsync() + 1;
        var orderNumber = $"BUY-{count:D4}";

        var order = new BuyerOrder
        {
            OrderNumber = orderNumber,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            CustomerEmail = customerEmail,
            DeliveryAddress = deliveryAddress,
            RequiredDate = requiredDate,
            Status = OrderStatus.PENDING,
            TotalAmount = 0,
            PaymentMethod = paymentMethod,
            IsPaid = false,
            CreatedAt = DateTime.UtcNow
        };

        decimal total = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in items)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);
            if (variant == null) throw new Exception($"Product variant {item.ProductVariantId} not found.");

            // SECURITY: use DB price, not client-supplied price
            var effectivePrice = variant.GuestPrice ?? variant.UnitPrice;

            var orderItem = new OrderItem
            {
                ProductVariantId = item.ProductVariantId,
                ProductVariant = variant,
                Quantity = item.Quantity,
                UnitPrice = effectivePrice
            };
            orderItems.Add(orderItem);
            total += item.Quantity * effectivePrice;
        }

        order.TotalAmount = total;
        order.Items = orderItems;

        _context.BuyerOrders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in items)
        {
            await _inventoryService.DeductStockAsync(
                item.ProductVariantId,
                item.Quantity,
                InventoryTransactionType.BuyerSale,
                $"Buyer Order {order.OrderNumber}",
                null
            );
        }

        return order;
    }

    public async Task<BuyerOrder> GetOrderAsync(Guid orderId)
    {
        return await _context.BuyerOrders
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new Exception("Order not found.");
    }

    public async Task<List<BuyerOrder>> GetOrdersAsync(OrderStatus? status = null)
    {
        var query = _context.BuyerOrders
            .Include(o => o.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .AsQueryable();
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<BuyerOrder> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, string? adminNotes = null)
    {
        var order = await GetOrderAsync(orderId);
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(adminNotes)) order.AdminNotes = adminNotes;
        await _context.SaveChangesAsync();
        await _notificationService.NotifyOrderStatusChangeAsync(order, adminNotes);
        return order;
    }

    public async Task<BuyerOrder> UpdatePaymentAsync(Guid orderId, string paymentReference, bool isPaid)
    {
        var order = await GetOrderAsync(orderId);
        order.PaymentReference = paymentReference;
        order.IsPaid = isPaid;
        order.UpdatedAt = DateTime.UtcNow;
        if (isPaid) order.Status = OrderStatus.PAID;
        await _context.SaveChangesAsync();
        return order;
    }
}
