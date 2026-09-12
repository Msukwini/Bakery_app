using BakeryApp.Api.DTOs;
using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;
    private readonly BakeryApp.Infrastructure.Data.BakeryDbContext _context;

    public DeliveryController(IDeliveryService deliveryService, BakeryApp.Infrastructure.Data.BakeryDbContext context)
    {
        _deliveryService = deliveryService;
        _context = context;
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignReseller([FromBody] AssignResellerRequest request)
    {
        try
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminId = !string.IsNullOrEmpty(adminIdClaim) ? Guid.Parse(adminIdClaim) : (Guid?)null;

            var assignment = await _deliveryService.AssignResellerAsync(
                request.ResellerEmployeeId,
                request.DeliveryEmployeeId,
                request.PermanentDeliveryEmployeeId,
                request.Type,
                request.StartDate,
                request.EndDate,
                request.Reason,
                adminId
            );

            return Ok(new { id = assignment.Id, message = "Assignment created successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// List all assignments (for admin dashboard)
    /// </summary>
    [HttpGet("assignments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListAllAssignments([FromQuery] bool activeOnly = true)
    {
        var list = await _deliveryService.GetAllAssignmentsAsync(activeOnly);
        var response = list.Select(a => new
        {
            id = a.Id,
            resellerEmployeeId = a.ResellerEmployeeId,
            resellerCode = a.Reseller?.Code ?? "Unknown",
            resellerName = a.Reseller?.Person != null
                ? $"{a.Reseller.Person.FirstName} {a.Reseller.Person.LastName}"
                : "Unknown",
            permanentDeliveryEmployeeId = a.PermanentDeliveryEmployeeId,
            permanentDeliveryCode = a.PermanentDeliveryEmployee?.Code ?? "Unknown",
            permanentDeliveryName = a.PermanentDeliveryEmployee?.Person != null
                ? $"{a.PermanentDeliveryEmployee.Person.FirstName} {a.PermanentDeliveryEmployee.Person.LastName}"
                : "Unknown",
            actualDeliveryEmployeeId = a.ActualDeliveryEmployeeId,
            actualDeliveryCode = a.ActualDeliveryEmployee?.Code,
            actualDeliveryName = a.ActualDeliveryEmployee?.Person != null
                ? $"{a.ActualDeliveryEmployee.Person.FirstName} {a.ActualDeliveryEmployee.Person.LastName}"
                : null,
            type = a.Type.ToString(),
            startDate = a.StartDate,
            endDate = a.EndDate,
            reason = a.Reason
        });
        return Ok(response);
    }

    [HttpGet("reseller/{resellerEmployeeId}/active")]
    [Authorize(Roles = "Admin,Reseller")]
    public async Task<IActionResult> GetActiveAssignmentsForReseller(Guid resellerEmployeeId, [FromQuery] DateTime? date)
    {
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        var currentUser = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null) return Unauthorized();

        if (currentUser.RoleType == EmployeeRoleType.Reseller && currentUser.Id != resellerEmployeeId)
            return Forbid("You can only view your own assignments.");

        var assignments = await _deliveryService.GetActiveAssignmentsForResellerAsync(resellerEmployeeId, date);

        var response = assignments.Select(a => new AssignmentResponse
        {
            Id = a.Id,
            ResellerCode = a.Reseller?.Code ?? "Unknown",
            PermanentDeliveryCode = a.PermanentDeliveryEmployee?.Code ?? "Unknown",
            ActualDeliveryCode = a.ActualDeliveryEmployee?.Code,
            Type = a.Type,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            Reason = a.Reason
        });

        return Ok(response);
    }

    [HttpGet("deliveryemployee/{deliveryEmployeeId}/assignments")]
    [Authorize(Roles = "Admin,Delivery")]
    public async Task<IActionResult> GetAssignmentsForDeliveryEmployee(Guid deliveryEmployeeId, [FromQuery] DateTime? date)
    {
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        var currentUser = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null) return Unauthorized();

        if (currentUser.RoleType == EmployeeRoleType.Delivery && currentUser.Id != deliveryEmployeeId)
            return Forbid("You can only view your own assignments.");

        var assignments = await _deliveryService.GetAssignmentsForDeliveryEmployeeAsync(deliveryEmployeeId, date);

        var response = assignments.Select(a => new AssignmentResponse
        {
            Id = a.Id,
            ResellerCode = a.Reseller?.Code ?? "Unknown",
            PermanentDeliveryCode = a.PermanentDeliveryEmployee?.Code ?? "Unknown",
            ActualDeliveryCode = a.ActualDeliveryEmployee?.Code,
            Type = a.Type,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            Reason = a.Reason
        });

        return Ok(response);
    }

    [HttpPut("assignment/{assignmentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAssignment(Guid assignmentId, [FromBody] UpdateAssignmentRequest request)
    {
        try
        {
            var assignment = await _deliveryService.UpdateAssignmentAsync(assignmentId, request.EndDate, request.Reason);
            return Ok(new { id = assignment.Id, message = "Assignment updated." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("reseller/{resellerEmployeeId}/history")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAssignmentHistory(Guid resellerEmployeeId)
    {
        var assignments = await _deliveryService.GetAssignmentHistoryForResellerAsync(resellerEmployeeId);
        var response = assignments.Select(a => new AssignmentResponse
        {
            Id = a.Id,
            ResellerCode = a.Reseller?.Code ?? "Unknown",
            PermanentDeliveryCode = a.PermanentDeliveryEmployee?.Code ?? "Unknown",
            ActualDeliveryCode = a.ActualDeliveryEmployee?.Code,
            Type = a.Type,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            Reason = a.Reason
        });
        return Ok(response);
    }

    [HttpPost("record-delivery")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecordDelivery([FromBody] RecordDeliveryRequest request)
    {
        try
        {
            var earning = await _deliveryService.RecordDeliveryCompletionAsync(
                request.DeliveryEmployeeId,
                request.CompletionDate,
                request.DailyRate
            );
            return Ok(new { id = earning.Id, message = "Delivery completion recorded." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("earnings/{deliveryEmployeeId}")]
    [Authorize(Roles = "Admin,Delivery")]
    public async Task<IActionResult> GetEarningsLedger(Guid deliveryEmployeeId)
    {
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        var currentUser = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null) return Unauthorized();

        if (currentUser.RoleType == EmployeeRoleType.Delivery && currentUser.Id != deliveryEmployeeId)
            return Forbid("You can only view your own earnings.");

        var entries = await _deliveryService.GetEarningsLedgerAsync(deliveryEmployeeId);
        var response = entries.Select(e => new DeliveryEarningResponse
        {
            Id = e.Id,
            DeliveryEmployeeCode = e.DeliveryEmployee?.Code ?? "Unknown",
            AmountEarned = e.AmountEarned,
            AmountPaid = e.AmountPaid,
            IsSettled = e.IsSettled,
            EarningDate = e.EarningDate
        });

        return Ok(response);
    }

    [HttpGet("balance/{deliveryEmployeeId}")]
    [Authorize(Roles = "Admin,Delivery")]
    public async Task<IActionResult> GetDeliveryBalance(Guid deliveryEmployeeId)
    {
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        var currentUser = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null) return Unauthorized();

        if (currentUser.RoleType == EmployeeRoleType.Delivery && currentUser.Id != deliveryEmployeeId)
            return Forbid("You can only view your own balance.");

        var outstanding = await _deliveryService.GetOutstandingEarningsAsync(deliveryEmployeeId);
        var entries = await _deliveryService.GetEarningsLedgerAsync(deliveryEmployeeId);
        var totalEarned = entries.Sum(e => e.AmountEarned);
        var totalPaid = entries.Sum(e => e.AmountPaid);

        return Ok(new DeliveryBalanceResponse
        {
            Outstanding = outstanding,
            TotalEarned = totalEarned,
            TotalPaid = totalPaid
        });
    }

    [HttpPost("payout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessDeliveryPayout([FromBody] DeliveryPayoutRequest request)
    {
        try
        {
            var employee = await _context.EmployeeIds
                .FirstOrDefaultAsync(e => e.Id == request.DeliveryEmployeeId && e.RoleType == EmployeeRoleType.Delivery);
            if (employee == null) return NotFound("Delivery employee not found.");

            var updatedEntries = await _deliveryService.ProcessDeliveryPayoutAsync(
                request.DeliveryEmployeeId,
                request.Amount,
                request.Notes
            );

            var newBalance = await _deliveryService.GetOutstandingEarningsAsync(request.DeliveryEmployeeId);

            return Ok(new DeliveryPayoutResponse
            {
                DeliveryEmployeeId = request.DeliveryEmployeeId,
                DeliveryEmployeeCode = employee.Code,
                AmountPaid = request.Amount,
                NewOutstandingBalance = newBalance,
                EntriesSettled = updatedEntries.Count,
                PayoutDate = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
