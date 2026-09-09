using BakeryApp.Core.Enums;

namespace BakeryApp.Api.DTOs;

public class RecordSaleRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceAtSale { get; set; }
    public Guid? ResellerEmployeeId { get; set; } // Optional: used by Admin
}

public class SaleResponse
{
    public Guid SaleId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CommissionEarned { get; set; }
    public DateTime SaleDate { get; set; }
}

public class CommissionLedgerResponse
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal AmountEarned { get; set; }
    public decimal AmountPaid { get; set; }
    public bool IsSettled { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? SaleId { get; set; }
}

public class CommissionBalanceResponse
{
    public decimal Outstanding { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalPaid { get; set; }
}
