using BakeryApp.Core.Enums;

namespace BakeryApp.Core.Entities;

public class Deposit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReferenceNumber { get; set; } = string.Empty; // DEP-YYYYMMDD-XXXX

    public Guid DeliveryEmployeeId { get; set; }
    public EmployeeId DeliveryEmployee { get; set; } = null!;

    public Guid? CashCollectionId { get; set; }
    public CashCollection? CashCollection { get; set; }

    public decimal Amount { get; set; }
    public DateTime DepositDate { get; set; }
    public string? BankName { get; set; }
    public string? BankReference { get; set; }

    public string? ProofFilePath { get; set; } // Path to uploaded image/PDF
    public string? ProofMimeType { get; set; }

    public DepositStatus Status { get; set; } = DepositStatus.SUBMITTED;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public EmployeeId? ReviewedByAdmin { get; set; }
    public string? RejectionReason { get; set; }
    public string? AdminNotes { get; set; }
}
