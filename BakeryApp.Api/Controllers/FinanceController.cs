using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class FinanceController : ControllerBase
{
    private readonly IFinancialService _financeService;

    public FinanceController(IFinancialService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _financeService.GetSummaryAsync(from, to);
        return Ok(result);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int year)
    {
        if (year == 0) year = DateTime.UtcNow.Year;
        var result = await _financeService.GetMonthlyBreakdownAsync(year);
        return Ok(result);
    }

    [HttpGet("stock-value")]
    public async Task<IActionResult> StockValue()
    {
        var value = await _financeService.GetStockValueAsync();
        return Ok(new { stockValue = value });
    }
}
