using Microsoft.AspNetCore.Mvc;

namespace BakeryApp.Api.Controllers;

public record RecordBatchRequest(Guid ProductVariantId, int Quantity, string BatchNumber);
public record WriteOffRequest(Guid ProductVariantId, int Quantity, string Reason);
public record StockLevelResponseDto(Guid ProductVariantId, int CurrentStockLevel);

[ApiController]
[Route("api/inventory")]
public class InventoryLedgerController : ControllerBase
{
    [HttpPost("batch")]
    public IActionResult RecordBatch([FromBody] RecordBatchRequest request)
    {
        return Ok();
    }

    [HttpPost("write-off")]
    public IActionResult WriteOff([FromBody] WriteOffRequest request)
    {
        return Ok();
    }

    [HttpGet("stock/{productVariantId:guid}")]
    public IActionResult GetStockLevel(Guid productVariantId)
    {
        return Ok(new StockLevelResponseDto(productVariantId, 45));
    }
}