using BakeryApp.Core.Enums;

namespace BakeryApp.Api.DTOs;

public class RecordCollectionDto
{
    public DateTime CollectionDate { get; set; }
    public decimal CollectedAmount { get; set; }
    public VarianceReason VarianceReason { get; set; } = VarianceReason.NONE;
    public string? VarianceNotes { get; set; }
}

public class ReconcileCollectionDto
{
    public string? AdminNotes { get; set; }
}

public class CollectionResponseDto
{
    public Guid Id { get; set; }
    public string DeliveryEmployeeCode { get; set; } = string.Empty;
    public DateTime CollectionDate { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal Variance { get; set; }
    public VarianceReason VarianceReason { get; set; }
    public CollectionStatus Status { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? AdminNotes { get; set; }
}

public class SubmitDepositDto
{
    public Guid? CashCollectionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DepositDate { get; set; }
    public string? BankName { get; set; }
    public string? BankReference { get; set; }
}

public class ReviewDepositDto
{
    public DepositStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}

public class DepositResponseDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string DeliveryEmployeeCode { get; set; } = string.Empty;
    public Guid? CashCollectionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DepositDate { get; set; }
    public string? BankName { get; set; }
    public string? BankReference { get; set; }
    public DepositStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}
