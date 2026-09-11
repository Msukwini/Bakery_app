namespace BakeryApp.Core.Enums;

public enum DepositStatus
{
    SUBMITTED,       // Delivery employee submitted proof
    UNDER_REVIEW,    // Admin is reviewing
    APPROVED,        // Admin verified
    REJECTED         // Admin rejected
}
