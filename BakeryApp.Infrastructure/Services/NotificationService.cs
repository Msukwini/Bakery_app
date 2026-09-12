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
    Task NotifyResellerSetupLinkAsync(string email, string firstName, string setupLink);
    Task SendPasswordResetEmailAsync(string email, string firstName, string resetLink);
    Task NotifyMilestoneReachedAsync(string resellerCode, string resellerName, string productName, string variantName, int units, decimal bonus, string periodKey);
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

    public async Task NotifyResellerSetupLinkAsync(string email, string firstName, string setupLink)
    {
        var subject = "Welcome to Ndlovu Bakery - Set Your Password";
        var body = "<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>" +
            "<h2 style='color:#1e40af;'>Welcome, " + firstName + "!</h2>" +
            "<p>Your reseller application has been approved.</p>" +
            "<p>Click the button below to set your password:</p>" +
            "<p style='text-align:center;margin:30px 0;'>" +
            "<a href='" + setupLink + "' style='background:#1e40af;color:white;padding:14px 28px;text-decoration:none;border-radius:8px;font-weight:bold;'>Set My Password</a>" +
            "</p>" +
            "<p style='color:#666;font-size:13px;'>Or copy this link: " + setupLink + "</p>" +
            "<p style='color:#999;font-size:12px;margin-top:30px;'>This link expires in 7 days.</p>" +
            "<hr style='border:none;border-top:1px solid #eee;margin:30px 0;'/>" +
            "<p style='color:#666;font-size:12px;'>Ndlovu Bakery - Durban, KZN</p>" +
            "</div>";
        await _email.SendAsync(email, subject, body, "RESELLER_SETUP_LINK");
    }

    public async Task SendPasswordResetEmailAsync(string email, string firstName, string resetLink)
    {
        var subject = "Reset Your Ndlovu Bakery Password";
        var body = "<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>" +
            "<h2 style='color:#1e40af;'>Password Reset</h2>" +
            "<p>Hi " + firstName + ",</p>" +
            "<p>We received a request to reset your password. Click the button below to set a new one:</p>" +
            "<p style='text-align:center;margin:30px 0;'>" +
            "<a href='" + resetLink + "' style='background:#1e40af;color:white;padding:14px 28px;text-decoration:none;border-radius:8px;font-weight:bold;'>Reset Password</a>" +
            "</p>" +
            "<p style='color:#666;font-size:13px;'>Or copy this link: " + resetLink + "</p>" +
            "<p style='color:#999;font-size:12px;margin-top:30px;'>This link expires in 1 hour. If you didn't request this, you can ignore this email.</p>" +
            "<hr style='border:none;border-top:1px solid #eee;margin:30px 0;'/>" +
            "<p style='color:#666;font-size:12px;'>Ndlovu Bakery - Durban, KZN</p>" +
            "</div>";
        await _email.SendAsync(email, subject, body, "PASSWORD_RESET");
    }

    public async Task NotifyMilestoneReachedAsync(string resellerCode, string resellerName, string productName, string variantName, int units, decimal bonus, string periodKey)
    {
        var subject = $"🎉 Milestone Reached - {resellerCode}";
        var body = "<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>" +
            "<h2 style='color:#16a34a;'>Milestone Achieved!</h2>" +
            "<p><strong>" + resellerName + " (" + resellerCode + ")</strong> has reached a sales milestone.</p>" +
            "<table style='border-collapse:collapse;margin:20px 0;'>" +
            "<tr><td style='padding:8px;color:#666;'>Product</td><td style='padding:8px;font-weight:bold;'>" + productName + " · " + variantName + "</td></tr>" +
            "<tr><td style='padding:8px;color:#666;'>Units Sold</td><td style='padding:8px;font-weight:bold;'>" + units + "</td></tr>" +
            "<tr><td style='padding:8px;color:#666;'>Period</td><td style='padding:8px;font-weight:bold;'>" + periodKey + "</td></tr>" +
            "<tr><td style='padding:8px;color:#666;'>Bonus Owed</td><td style='padding:8px;font-weight:bold;color:#16a34a;'>R " + bonus.ToString("F2") + "</td></tr>" +
            "</table>" +
            "<p>Review and pay the bonus in the Milestones dashboard.</p>" +
            "<hr style='border:none;border-top:1px solid #eee;margin:30px 0;'/>" +
            "<p style='color:#666;font-size:12px;'>Ndlovu Bakery - Durban, KZN</p>" +
            "</div>";
        await _email.SendAsync(AdminEmail, subject, body, "MILESTONE_REACHED");
    }

    private async Task<string?> GetResellerEmailAsync(Guid resellerEmployeeId)
    {
        var emp = await _context.EmployeeIds.Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId);
        return emp?.Person?.Email;
    }
}
