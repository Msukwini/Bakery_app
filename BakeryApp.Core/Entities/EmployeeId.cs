namespace BakeryApp.Core.Entities;

using BakeryApp.Core.Enums;

public class EmployeeId
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public EmployeeRoleType RoleType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Reseller lifecycle (only for RoleType = Reseller)
    public ResellerStatus? ResellerStatus { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public Guid? StatusChangedByAdminId { get; set; }
    public DateTime? TrialEndsAt { get; set; }

    public Guid PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public Guid? ResidenceId { get; set; }
    public Residence? Residence { get; set; }
}
