using BakeryApp.Core.Entities;

namespace BakeryApp.Infrastructure.Services;

public interface INotificationService
{
    Task SendOrderStatusUpdateAsync(BuyerOrder order, string? adminNotes = null);
    Task SendResellerApplicationStatusAsync(ResellerApplication application, string? adminNotes = null);
    Task SendDeliveryPayoutNotificationAsync(string deliveryEmployeeCode, decimal amount);
}

public class ConsoleNotificationService : INotificationService
{
    public Task SendOrderStatusUpdateAsync(BuyerOrder order, string? adminNotes = null)
    {
        Console.WriteLine($"[NOTIFICATION] Order {order.OrderNumber} status changed to {order.Status}. Notes: {adminNotes ?? "N/A"}");
        Console.WriteLine($"            Customer: {order.CustomerName} ({order.CustomerEmail})");
        return Task.CompletedTask;
    }

    public Task SendResellerApplicationStatusAsync(ResellerApplication application, string? adminNotes = null)
    {
        Console.WriteLine($"[NOTIFICATION] Reseller application {application.Id} status changed to {application.Status}. Notes: {adminNotes ?? "N/A"}");
        Console.WriteLine($"            Applicant: {application.FirstName} {application.LastName} ({application.Email})");
        return Task.CompletedTask;
    }

    public Task SendDeliveryPayoutNotificationAsync(string deliveryEmployeeCode, decimal amount)
    {
        Console.WriteLine($"[NOTIFICATION] Delivery payout of {amount} processed for {deliveryEmployeeCode}");
        return Task.CompletedTask;
    }
}
