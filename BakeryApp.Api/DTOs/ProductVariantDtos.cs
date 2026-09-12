namespace BakeryApp.Api.DTOs;

public class UpdateVariantPricingDto
{
    public decimal? UnitPrice { get; set; }
    public decimal? GuestPrice { get; set; }
    public decimal? StockCost { get; set; }
    public bool? IsActive { get; set; }
}
