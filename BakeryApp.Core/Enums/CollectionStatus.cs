namespace BakeryApp.Core.Enums;

public enum CollectionStatus
{
    RECORDED,       // Delivery employee recorded a collection
    VARIANCE_FLAGGED, // Amount differs from expected
    RECONCILED      // Admin reviewed and reconciled
}
