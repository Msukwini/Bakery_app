using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface INotificationService
{
    Task NotifyOrderStatusChangeAsync(BuyerOrder order, string? adminNotes = null);
    Task NotifyDepositSubmittedAsync(Deposit deposit);
    Task NotifyStockRequestSubmittedAsync(ResellerStockRequest request);
    Task NotifyStockRequestApprovedAsync(ResellerStockRequest request);
    Task NotifyStockRequestRejectedAsync(ResellerStockRequest request);
    Task NotifyLowStockAsync(string productName, string variantName, int currentStock);
    Task NotifyResellerApplicationSubmittedAsync(ResellerApplication application);
}

public class NotificationService : INotificationService
{
    private readonly BakeryDbContext _context;
    private readonly IEmailService _email;
    private const string AdminEmail = "bakeryndlovu@gmail.com";

    public NotificationService(BakeryDbContext context, IEmailService email)
    {
        _context = context;
        _email = email;
    }

    public async Task NotifyOrderStatusChangeAsync(BuyerOrder order, string? adminNotes = null)
    {
        if (string.IsNullOrEmpty(order.CustomerEmail)) return;
        var subject = $"Order {order.OrderNumber} - {order.Status}";
        var body = $"<h2>Order Update</h2><p>Your order <strong>{order.OrderNumber}</strong> is now <strong>{order.Status}</strong>.</p><p>Total: R {order.TotalAmount:F2}</p>";
        await _email.SendAsync(order.CustomerEmail, subject, body, "ORDER_STATUS");
    }

    public async Task NotifyDepositSubmittedAsync(Deposit deposit)
    {
        var subject = $"New Deposit Submitted - {deposit.ReferenceNumber}";
        var body = $"<h2>New Deposit</h2><p>Reference: {deposit.ReferenceNumber}</p><p>Amount: R {deposit.Amount:F2}</p>";
        await _email.SendAsync(AdminEmail, subject, body, "DEPOSIT_SUBMITTED");
    }

    public async Task NotifyStockRequestSubmittedAsync(ResellerStockRequest request)
    {
        var subject = $"New Stock Request from {request.Reseller?.Code}";
        var body = $"<h2>New Stock Request</h2><p>Reseller: {request.Reseller?.Code}</p><p>Quantity: {request.RequestedQuantity}</p>";
        await _email.SendAsync(AdminEmail, subject, body, "STOCK_REQUEST_SUBMITTED");
    }

    public async Task NotifyStockRequestApprovedAsync(ResellerStockRequest request)
    {
        var resellerEmail = await GetResellerEmailAsync(request.ResellerEmployeeId);
        if (string.IsNullOrEmpty(resellerEmail)) return;
        var subject = "Stock Request Approved";
        var body = $"<h2>Approved</h2><p>Your request for {request.AllocatedQuantity}x {request.ProductVariant?.Product?.Name} has been approved.</p>";
        await _email.SendAsync(resellerEmail, subject, body, "STOCK_REQUEST_APPROVED");
    }

    public async Task NotifyStockRequestRejectedAsync(ResellerStockRequest request)
    {
        var resellerEmail = await GetResellerEmailAsync(request.ResellerEmployeeId);
        if (string.IsNullOrEmpty(resellerEmail)) return;
        var subject = "Stock Request Rejected";
        var body = $"<h2>Rejected</h2><p>Reason: {request.RejectionReason ?? "Not specified"}</p>";
        await _email.SendAsync(resellerEmail, subject, body, "STOCK_REQUEST_REJECTED");
    }

    public async Task NotifyLowStockAsync(string productName, string variantName, int currentStock)
    {
        var subject = $"Low Stock Alert: {productName}";
        var body = $"<h2>Low Stock</h2><p>{productName} ({variantName}): {currentStock} remaining</p>";
        await _email.SendAsync(AdminEmail, subject, body, "LOW_STOCK");
    }

    public async Task NotifyResellerApplicationSubmittedAsync(ResellerApplication application)
    {
        var subject = $"New Reseller Application - {application.FirstName} {application.LastName}";
        var body = $"<h2>New Application</h2><p>{application.FirstName} {application.LastName}</p><p>Email: {application.Email}</p><p>Phone: {application.PhoneNumber}</p>";
        await _email.SendAsync(AdminEmail, subject, body, "RESELLER_APPLICATION");
    }

    private async Task<string?> GetResellerEmailAsync(Guid resellerEmployeeId)
    {
        var emp = await _context.EmployeeIds.Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId);
        return emp?.Person?.Email;
    }
}
