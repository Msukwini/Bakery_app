namespace BakeryApp.Core.Entities;

using BakeryApp.Core.Enums;

public class DeliveryAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ResellerEmployeeId { get; set; }
    public EmployeeId Reseller { get; set; } = null!;
    
    public Guid PermanentDeliveryEmployeeId { get; set; } // The owner
    public EmployeeId PermanentDeliveryEmployee { get; set; } = null!;
    
    public Guid? ActualDeliveryEmployeeId { get; set; } // Who actually did it (can be temp)
    public EmployeeId? ActualDeliveryEmployee { get; set; }
    
    public AssignmentType Type { get; set; } // PERMANENT or TEMPORARY
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? CreatedByAdminId { get; set; }
}
