namespace BakeryApp.Core.Enums;

public enum InventoryTransactionType
{
    ProductionBatch = 1,
    AllocatedToReseller = 2,
    SaleConfirmed = 3,
    DamagedOrReturned = 4,
    AuditAdjustment = 5
}