using BakeryApp.Api.DTOs;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/admin/reseller-lifecycle")]
[Authorize(Roles = "Admin")]
public class ResellerLifecycleController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public ResellerLifecycleController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ResellerStatus? status)
    {
        var query = _context.EmployeeIds
            .Include(e => e.Person)
            .Include(e => e.Residence)
            .Where(e => e.RoleType == EmployeeRoleType.Reseller);

        if (status.HasValue)
            query = query.Where(e => e.ResellerStatus == status.Value);

        var list = await query
            .OrderByDescending(e => e.AssignedAt)
            .ToListAsync();

        var response = list.Select(e => new ResellerLifecycleResponse
        {
            EmployeeId = e.Id,
            Code = e.Code,
            PersonId = e.PersonId,
            FullName = $"{e.Person.FirstName} {e.Person.LastName}",
            Email = e.Person.Email,
            Phone = e.Person.PhoneNumber,
            ResidenceName = e.Residence?.Name,
            Status = e.ResellerStatus,
            StatusReason = e.StatusReason,
            StatusChangedAt = e.StatusChangedAt,
            TrialEndsAt = e.TrialEndsAt,
            IsActive = e.IsActive,
            AssignedAt = e.AssignedAt
        });

        return Ok(response);
    }

    [HttpPut("{employeeId}/status")]
    public async Task<IActionResult> ChangeStatus(Guid employeeId, [FromBody] ChangeResellerStatusDto dto)
    {
        var emp = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.RoleType == EmployeeRoleType.Reseller);
        if (emp == null) return NotFound(new { error = "Reseller not found." });

        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        Guid? adminId = null;
        if (!string.IsNullOrEmpty(adminCode))
        {
            var admin = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == adminCode);
            adminId = admin?.Id;
        }

        var oldStatus = emp.ResellerStatus;

        emp.ResellerStatus = dto.Status;
        emp.StatusReason = dto.Reason;
        emp.StatusChangedAt = DateTime.UtcNow;
        emp.StatusChangedByAdminId = adminId;

        emp.IsActive = dto.Status == ResellerStatus.ACTIVE || dto.Status == ResellerStatus.TRIAL;

        if (dto.Status == ResellerStatus.TRIAL)
        {
            var days = dto.TrialDays ?? 30;
            emp.TrialEndsAt = DateTime.UtcNow.AddDays(days);
        }
        else
        {
            emp.TrialEndsAt = null;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Status changed from {oldStatus?.ToString() ?? "N/A"} to {dto.Status}.",
            employeeId = emp.Id,
            newStatus = dto.Status.ToString(),
            isActive = emp.IsActive
        });
    }
}
