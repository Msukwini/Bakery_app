using BakeryApp.Core.Entities;

namespace BakeryApp.Core.Entities;

public class DeliveryEarning
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryEmployeeId { get; set; } // EmployeeId with RoleType = Delivery
    public EmployeeId DeliveryEmployee { get; set; } = null!;
    
    public Guid DeliveryAssignmentId { get; set; }
    public DeliveryAssignment DeliveryAssignment { get; set; } = null!;
    
    public decimal AmountEarned { get; set; }
    public decimal AmountPaid { get; set; }
    public bool IsSettled { get; set; }
    public DateTime EarningDate { get; set; } = DateTime.UtcNow;
}
