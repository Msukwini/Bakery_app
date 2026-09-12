using BakeryApp.Api.DTOs;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var items = request.Items
                .Select(i => (i.ProductVariantId, i.Quantity))
                .ToList();

            var order = await _orderService.CreateOrderAsync(
                request.CustomerName,
                request.CustomerPhone,
                request.CustomerEmail,
                request.DeliveryAddress,
                request.RequiredDate,
                items,
                request.PaymentMethod
            );

            return Ok(MapOrder(order));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> List([FromQuery] OrderStatus? status)
    {
        var orders = await _orderService.GetOrdersAsync(status);
        return Ok(orders.Select(MapOrder));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id);
            return Ok(MapOrder(order));
        }
        catch { return NotFound(); }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request.Status, request.AdminNotes);
            return Ok(MapOrder(order));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}/payment")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdateOrderPaymentRequest request)
    {
        try
        {
            var order = await _orderService.UpdatePaymentAsync(id, request.PaymentReference, request.IsPaid);
            return Ok(MapOrder(order));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    private static OrderResponse MapOrder(Core.Entities.BuyerOrder o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerName = o.CustomerName,
        CustomerPhone = o.CustomerPhone,
        CustomerEmail = o.CustomerEmail,
        DeliveryAddress = o.DeliveryAddress,
        RequiredDate = o.RequiredDate,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        PaymentMethod = o.PaymentMethod,
        PaymentReference = o.PaymentReference,
        IsPaid = o.IsPaid,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        AdminNotes = o.AdminNotes,
        Items = o.Items.Select(i => new OrderItemResponse
        {
            Id = i.Id,
            ProductVariantId = i.ProductVariantId,
            ProductName = i.ProductVariant?.Product?.Name ?? "Unknown",
            VariantName = i.ProductVariant?.SizeName ?? "Unknown",
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice
        }).ToList()
    };
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? AdminNotes { get; set; }
}

public class UpdateOrderPaymentRequest
{
    public string PaymentReference { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
}
