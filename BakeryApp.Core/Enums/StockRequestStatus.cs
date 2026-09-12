namespace BakeryApp.Core.Enums;

public enum StockRequestStatus
{
    PENDING,
    APPROVED,
    ALLOCATED,
    RECEIVED,
    REJECTED,
    CANCELLED,
    DELIVERED,          // Driver marked as delivered
    VARIANCE_PENDING,   // Quantity mismatch between driver and reseller
    DISPUTED            // Stock in dispute, cannot be sold
}
