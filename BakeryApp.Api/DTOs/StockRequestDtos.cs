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
    public int? AllocatedQuantity { get; set; } // required for approval
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
}

public class StockRequestResponseDto
{
    public Guid Id { get; set; }
    public string ResellerCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int? AllocatedQuantity { get; set; }
    public StockRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? AllocatedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? ResellerNotes { get; set; }
}

public class ResellerStockAccountabilityDto
{
    public Guid ResellerEmployeeId { get; set; }
    public string ResellerCode { get; set; } = string.Empty;
    public int OpeningStock { get; set; }
    public int TotalReceived { get; set; }
    public int TotalSold { get; set; }
    public int TotalDamaged { get; set; }
    public int TotalReturned { get; set; }
    public int ExpectedClosingStock { get; set; }
    public List<ResellerStockItemDto> Items { get; set; } = new();
}

public class ResellerStockItemDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Sold { get; set; }
    public int Damaged { get; set; }
    public int Returned { get; set; }
    public int ExpectedStock { get; set; }
}
