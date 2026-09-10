using BakeryApp.Core.Enums;

namespace BakeryApp.Core.Entities;

public class ResellerApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ResidenceName { get; set; } // Applicant's current residence (string for flexibility)
    public string? RoomNumber { get; set; }
    public int? EstimatedResidencePopulation { get; set; }
    public string? PreferredSellingArea { get; set; }
    public string? PreviousSalesExperience { get; set; }
    public string? Availability { get; set; }
    public string? ExpectedTimeAtResidence { get; set; }
    public string? AdditionalInfo { get; set; }

    // The residence the applicant wants to be assigned to (optional at submission, set by admin)
    public Guid? ResidenceId { get; set; }
    public Residence? Residence { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.PENDING;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? AdminNotes { get; set; }

    // Admin who reviewed/approved/rejected
    public Guid? ReviewedByAdminId { get; set; }
    public EmployeeId? ReviewedByAdmin { get; set; }

    // If approved, the generated EmployeeId will be linked here
    public Guid? ApprovedEmployeeId { get; set; }
    public EmployeeId? ApprovedEmployee { get; set; }
}
