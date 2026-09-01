namespace BakeryApp.Core.Entities;

using BakeryApp.Core.Enums;

public class EmployeeId
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty; // Unique badge/code (e.g., "RES-1042" or "DEL-0089")
    public EmployeeRoleType RoleType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Direct link to the parent Person record
    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    // Optional link for Reseller roles bound to a residence
    public Guid? ResidenceId { get; set; }
    public Residence? Residence { get; set; }
}