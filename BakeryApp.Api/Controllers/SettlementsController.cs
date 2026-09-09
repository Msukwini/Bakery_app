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
public class SettlementsController : ControllerBase
{
    private readonly ISettlementService _settlementService;
    private readonly BakeryApp.Infrastructure.Data.BakeryDbContext _context;

    public SettlementsController(ISettlementService settlementService, BakeryApp.Infrastructure.Data.BakeryDbContext context)
    {
        _settlementService = settlementService;
        _context = context;
    }

    [HttpPost("payout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessPayout([FromBody] PayoutRequest request)
    {
        try
        {
            var reseller = await _context.EmployeeIds
                .FirstOrDefaultAsync(e => e.Id == request.ResellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller);
            if (reseller == null) return NotFound("Reseller not found.");

            var updatedEntries = await _settlementService.ProcessPayoutAsync(
                request.ResellerEmployeeId,
                request.Amount,
                request.Notes
            );

            var newBalance = await _settlementService.GetOutstandingBalanceAsync(request.ResellerEmployeeId);

            return Ok(new PayoutResponse
            {
                ResellerEmployeeId = request.ResellerEmployeeId,
                ResellerCode = reseller.Code,
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

    [HttpGet("balance/{resellerEmployeeId}")]
    [Authorize(Roles = "Admin,Reseller")]
    public async Task<IActionResult> GetBalance(Guid resellerEmployeeId)
    {
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        var currentUser = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null) return Unauthorized();

        if (currentUser.RoleType == EmployeeRoleType.Reseller && currentUser.Id != resellerEmployeeId)
            return Forbid("You can only view your own balance.");

        var outstanding = await _settlementService.GetOutstandingBalanceAsync(resellerEmployeeId);
        var totalEarned = await _settlementService.GetTotalEarnedAsync(resellerEmployeeId);
        var totalPaid = await _settlementService.GetTotalPaidAsync(resellerEmployeeId);

        return Ok(new SettlementBalanceResponse
        {
            Outstanding = outstanding,
            TotalEarned = totalEarned,
            TotalPaid = totalPaid
        });
    }
}
