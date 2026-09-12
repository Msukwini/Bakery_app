using BakeryApp.Core.Enums;

namespace BakeryApp.Api.DTOs;

public class ChangeResellerStatusDto
{
    public ResellerStatus Status { get; set; }
    public string? Reason { get; set; }
    public int? TrialDays { get; set; }
}

public class ResellerLifecycleResponse
{
    public Guid EmployeeId { get; set; }
    public string Code { get; set; } = "";
    public Guid PersonId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? ResidenceName { get; set; }
    public ResellerStatus? Status { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public DateTime? TrialEndsAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}
