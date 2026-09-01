namespace BakeryApp.Api.DTOs;

using BakeryApp.Core.Enums;

public record CreateProductionBatchDto(
    Guid ProductVariantId,
    int Quantity,
    string ReferenceNote
);

public record CreateAllocationDto(
    Guid ProductVariantId,
    Guid ResellerEmployeeId,
    int Quantity,
    string ReferenceNote
);

public record CreateAdjustmentDto(
    Guid ProductVariantId,
    InventoryTransactionType TransactionType,
    int Quantity,
    string ReferenceNote
);

public record InventoryLedgerResponseDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductName,
    string SizeName,
    InventoryTransactionType TransactionType,
    string TransactionTypeLabel,
    int Quantity,
    DateTime Timestamp,
    string ReferenceNote,
    Guid? EmployeeId,
    string? EmployeeCode,
    string? EmployeeName
);

public record StockBalanceDto(
    Guid ProductVariantId,
    string ProductName,
    string SizeName,
    int AvailableStock
);