using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("orders/csv")]
    public async Task<IActionResult> ExportOrders(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] OrderStatus? status)
    {
        var data = await _reportService.ExportOrdersCsvAsync(from, to, status);
        return File(data, "text/csv", $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("commissions/csv")]
    public async Task<IActionResult> ExportCommissions(
        [FromQuery] Guid? resellerEmployeeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var data = await _reportService.ExportCommissionLedgerCsvAsync(resellerEmployeeId, from, to);
        return File(data, "text/csv", $"Commissions_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet("delivery/csv")]
    public async Task<IActionResult> ExportDeliveryEarnings(
        [FromQuery] Guid? deliveryEmployeeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var data = await _reportService.ExportDeliveryEarningsCsvAsync(deliveryEmployeeId, from, to);
        return File(data, "text/csv", $"DeliveryEarnings_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
}
