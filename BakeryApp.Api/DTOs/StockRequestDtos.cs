using BakeryApp.Core.Enums;

namespace BakeryApp.Api.DTOs;

public class CreateStockRequestDto
{
    public Guid ProductVariantId { get; set; }
    public int RequestedQuantity { get; set; }
    public string? ResellerNotes { get; set; }
}

public class ReviewStockRequestDto
{
    public int? AllocatedQuantity { get; set; }
    public Guid? AssignedDeliveryEmployeeId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}

public class MarkDeliveredDto
{
    public int DeliveredQuantity { get; set; }
    public string? DeliveryNotes { get; set; }
}

public class ConfirmReceiptDto
{
    public int ReceivedQuantity { get; set; }
    public string? ReceiptNotes { get; set; }
}

public class ResolveVarianceDto
{
    public string ResolutionNotes { get; set; } = string.Empty;
    public bool WriteOff { get; set; } = false;
}

public class StockRequestResponseDto
{
    public Guid Id { get; set; }
    public string ResellerCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;

    public int RequestedQuantity { get; set; }
    public int? AllocatedQuantity { get; set; }
    public int? DeliveredQuantity { get; set; }
    public int? ReceivedQuantity { get; set; }

    public StockRequestStatus Status { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? AllocatedAt { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DeliveryCompletedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public string? AssignedDeliveryCode { get; set; }
    public string? AssignedDeliveryName { get; set; }
    public string? ActualDeliveryCode { get; set; }
    public string? ActualDeliveryName { get; set; }

    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? ResellerNotes { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? ReceiptNotes { get; set; }

    public int? Variance { get; set; }
    public VarianceStatus VarianceStatus { get; set; }
    public string? VarianceNotes { get; set; }
}
