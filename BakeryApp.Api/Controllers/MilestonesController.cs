using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BakeryApp.Api.Controllers;

public class CreateMilestoneRequest
{
    public Guid ProductVariantId { get; set; }
    public int TargetUnits { get; set; }
    public decimal BonusAmount { get; set; }
    public MilestoneTimeframe Timeframe { get; set; } = MilestoneTimeframe.Monthly;
}

public class MarkPaidRequest
{
    public string? Notes { get; set; }
}

[ApiController]
[Route("api/admin/milestones")]
[Authorize(Roles = "Admin")]
public class MilestonesController : ControllerBase
{
    private readonly IMilestoneService _milestoneService;

    public MilestonesController(IMilestoneService milestoneService)
    {
        _milestoneService = milestoneService;
    }

    [HttpGet("configs")]
    public async Task<IActionResult> ListConfigs()
    {
        var configs = await _milestoneService.GetAllConfigsAsync();
        return Ok(configs.Select(c => new
        {
            c.Id,
            c.ProductVariantId,
            ProductName = c.ProductVariant?.Product?.Name ?? "Unknown",
            VariantName = c.ProductVariant?.SizeName ?? "Unknown",
            c.TargetUnits,
            c.BonusAmount,
            Timeframe = c.Timeframe.ToString(),
            c.IsActive,
            c.CreatedAt
        }));
    }

    [HttpPost("configs")]
    public async Task<IActionResult> CreateConfig([FromBody] CreateMilestoneRequest req)
    {
        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        Guid? adminId = null;

        try
        {
            var config = await _milestoneService.CreateConfigAsync(
                req.ProductVariantId,
                req.TargetUnits,
                req.BonusAmount,
                req.Timeframe,
                adminId
            );
            return Ok(new { id = config.Id, message = "Milestone config created." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("configs/{id}")]
    public async Task<IActionResult> DeleteConfig(Guid id)
    {
        try
        {
            await _milestoneService.DeleteConfigAsync(id);
            return Ok(new { message = "Milestone config deactivated." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> ListAlerts([FromQuery] bool? paid)
    {
        var alerts = await _milestoneService.GetAlertsAsync(paid);
        return Ok(alerts.Select(a => new
        {
            a.Id,
            ResellerEmployeeId = a.ResellerEmployeeId,
            ResellerCode = a.Reseller?.Code ?? "Unknown",
            ResellerName = a.Reseller?.Person != null
                ? $"{a.Reseller.Person.FirstName} {a.Reseller.Person.LastName}"
                : "Unknown",
            ResellerPersonId = a.Reseller?.Person?.Id,
            ResellerHasPicture = a.Reseller?.Person != null && !string.IsNullOrEmpty(a.Reseller.Person.ProfilePicturePath),
            ProductName = a.MilestoneConfig?.ProductVariant?.Product?.Name ?? "Unknown",
            VariantName = a.MilestoneConfig?.ProductVariant?.SizeName ?? "Unknown",
            TargetUnits = a.MilestoneConfig?.TargetUnits ?? 0,
            a.AchievedUnits,
            a.BonusOwed,
            a.PeriodKey,
            a.TriggeredAt,
            a.IsPaid,
            a.PaidAt,
            a.AdminNotes
        }));
    }

    [HttpPost("alerts/{id}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaidRequest req)
    {
        try
        {
            await _milestoneService.MarkPaidAsync(id, req.Notes);
            return Ok(new { message = "Marked as paid." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
