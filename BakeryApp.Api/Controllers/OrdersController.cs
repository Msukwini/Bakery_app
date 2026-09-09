using BakeryApp.Api.DTOs;
using BakeryApp.Core.Entities;
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
    [AllowAnonymous] // Guest checkout
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var items = request.Items.Select(i => (i.ProductVariantId, i.Quantity, i.UnitPrice)).ToList();
            var order = await _orderService.CreateOrderAsync(
                request.CustomerName,
                request.CustomerPhone,
                request.CustomerEmail,
                request.DeliveryAddress,
                request.RequiredDate,
                items,
                request.PaymentMethod
            );

            var response = MapOrderToResponse(order);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status)
    {
        var orders = await _orderService.GetOrdersAsync(status);
        var response = orders.Select(MapOrderToResponse);
        return Ok(response);
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // Or Authorize – guest can view with order number? We'll use ID for simplicity.
    public async Task<IActionResult> GetOrder(Guid id)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(id);
            return Ok(MapOrderToResponse(order));
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, request.Status, request.AdminNotes);
            return Ok(MapOrderToResponse(order));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/payment")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentRequest request)
    {
        try
        {
            var order = await _orderService.UpdatePaymentAsync(id, request.PaymentReference, request.IsPaid);
            return Ok(MapOrderToResponse(order));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private OrderResponse MapOrderToResponse(BuyerOrder order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            DeliveryAddress = order.DeliveryAddress,
            RequiredDate = order.RequiredDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentReference = order.PaymentReference,
            IsPaid = order.IsPaid,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            AdminNotes = order.AdminNotes,
            Items = order.Items.Select(i => new OrderItemResponse
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
}
