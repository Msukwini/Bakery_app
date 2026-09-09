namespace BakeryApp.Api.DTOs;

public class PayoutRequest
{
    public Guid ResellerEmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class PayoutResponse
{
    public Guid ResellerEmployeeId { get; set; }
    public string ResellerCode { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public decimal NewOutstandingBalance { get; set; }
    public int EntriesSettled { get; set; }
    public DateTime PayoutDate { get; set; }
}

public class SettlementBalanceResponse
{
    public decimal Outstanding { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalPaid { get; set; }
}
