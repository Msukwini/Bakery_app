using BakeryApp.Core.Enums;

namespace BakeryApp.Api.DTOs;

public class AddStockRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public string? ReferenceNote { get; set; }
    public decimal? UnitCost { get; set; }
}

public class DeductStockRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public InventoryTransactionType TransactionType { get; set; }
    public string? ReferenceNote { get; set; }
}

public class StockResponse
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
}

public class LedgerEntryResponse
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public InventoryTransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public DateTime Timestamp { get; set; }
    public string ReferenceNote { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
}
