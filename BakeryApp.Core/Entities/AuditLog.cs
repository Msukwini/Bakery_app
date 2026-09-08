namespace BakeryApp.Core.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; } // Could be PersonId or EmployeeId
    public string Action { get; set; } = string.Empty; // "APPROVED_APPLICATION", "CHANGED_PRICE"
    public string EntityType { get; set; } = string.Empty; // "Reseller", "Product"
    public string? EntityId { get; set; }
    public string? OldValue { get; set; } // JSON
    public string? NewValue { get; set; } // JSON
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
