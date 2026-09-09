using BakeryApp.Core.Enums;

namespace BakeryApp.Api.DTOs;

public class AssignResellerRequest
{
    public Guid ResellerEmployeeId { get; set; }
    public Guid DeliveryEmployeeId { get; set; }
    public AssignmentType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}

public class UpdateAssignmentRequest
{
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}

public class AssignmentResponse
{
    public Guid Id { get; set; }
    public string ResellerCode { get; set; } = string.Empty;
    public string PermanentDeliveryCode { get; set; } = string.Empty;
    public string? ActualDeliveryCode { get; set; }
    public AssignmentType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RecordDeliveryRequest
{
    public Guid DeliveryEmployeeId { get; set; }
    public DateTime CompletionDate { get; set; }
    public decimal? DailyRate { get; set; }
}

public class DeliveryEarningResponse
{
    public Guid Id { get; set; }
    public string DeliveryEmployeeCode { get; set; } = string.Empty;
    public decimal AmountEarned { get; set; }
    public decimal AmountPaid { get; set; }
    public bool IsSettled { get; set; }
    public DateTime EarningDate { get; set; }
}

public class DeliveryBalanceResponse
{
    public decimal Outstanding { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalPaid { get; set; }
}

// NEW: Payout DTOs
public class DeliveryPayoutRequest
{
    public Guid DeliveryEmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class DeliveryPayoutResponse
{
    public Guid DeliveryEmployeeId { get; set; }
    public string DeliveryEmployeeCode { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public decimal NewOutstandingBalance { get; set; }
    public int EntriesSettled { get; set; }
    public DateTime PayoutDate { get; set; }
}
