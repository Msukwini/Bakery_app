using BakeryApp.Core.Enums;

namespace BakeryApp.Api.DTOs;

public class SubmitApplicationRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ResidenceName { get; set; }
    public string? RoomNumber { get; set; }
    public int? EstimatedResidencePopulation { get; set; }
    public string? PreferredSellingArea { get; set; }
    public string? PreviousSalesExperience { get; set; }
    public string? Availability { get; set; }
    public string? ExpectedTimeAtResidence { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
}

public class ReviewApplicationRequest
{
    public ApplicationStatus Status { get; set; }
    public Guid? ResidenceId { get; set; }
    public string? AdminNotes { get; set; }
}

public class ApplicationResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ResidenceName { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? AdminNotes { get; set; }
    public string? EmployeeIdCode { get; set; }
}
