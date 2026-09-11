using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

public class AssignRoleRequest
{
    public Guid? ResidenceId { get; set; } // Required for Reseller only
}

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class RoleManagementController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public RoleManagementController(BakeryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all Applicants (people who registered but have no Reseller/Delivery role yet)
    /// </summary>
    [HttpGet("pending-users")]
    public async Task<IActionResult> ListPendingUsers()
    {
        var applicants = await _context.EmployeeIds
            .Include(e => e.Person)
            .Where(e => e.RoleType == EmployeeRoleType.Applicant && e.IsActive)
            .OrderByDescending(e => e.AssignedAt)
            .Select(e => new
            {
                employeeId = e.Id,
                personId = e.PersonId,
                code = e.Code,
                firstName = e.Person.FirstName,
                lastName = e.Person.LastName,
                email = e.Person.Email,
                phone = e.Person.PhoneNumber,
                registeredAt = e.AssignedAt
            })
            .ToListAsync();

        return Ok(applicants);
    }

    /// <summary>
    /// List all Persons with their current roles (for admin to see who has what)
    /// </summary>
    [HttpGet("people")]
    public async Task<IActionResult> ListAllPeople()
    {
        var people = await _context.Persons
            .Include(p => p.EmployeeIds)
            .OrderBy(p => p.FirstName)
            .Select(p => new
            {
                personId = p.Id,
                firstName = p.FirstName,
                lastName = p.LastName,
                email = p.Email,
                phone = p.PhoneNumber,
                roles = p.EmployeeIds
                    .Where(e => e.RoleType != EmployeeRoleType.Applicant && e.IsActive)
                    .Select(e => new
                    {
                        employeeId = e.Id,
                        code = e.Code,
                        role = e.RoleType.ToString(),
                        residenceId = e.ResidenceId
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(people);
    }

    /// <summary>
    /// Approve applicant as Reseller (adds RES-XXXX EmployeeId)
    /// </summary>
    [HttpPost("people/{personId}/assign-reseller")]
    public async Task<IActionResult> AssignReseller(Guid personId, [FromBody] AssignRoleRequest req)
    {
        if (req.ResidenceId == null)
            return BadRequest(new { error = "ResidenceId is required." });

        var person = await _context.Persons
            .Include(p => p.EmployeeIds)
            .FirstOrDefaultAsync(p => p.Id == personId);
        if (person == null) return NotFound(new { error = "Person not found." });

        // Check if they already have an active Reseller role
        var existingReseller = person.EmployeeIds
            .FirstOrDefault(e => e.RoleType == EmployeeRoleType.Reseller && e.IsActive);
        if (existingReseller != null)
            return BadRequest(new { error = $"Already has Reseller role: {existingReseller.Code}" });

        // Verify residence exists
        var residence = await _context.Residences.FindAsync(req.ResidenceId.Value);
        if (residence == null)
            return BadRequest(new { error = "Residence not found." });

        // Generate unique RES code
        var resellerCount = await _context.EmployeeIds.CountAsync(e => e.RoleType == EmployeeRoleType.Reseller);
        var newCode = $"RES-{(resellerCount + 1):D4}";

        // Deactivate the Applicant role
        var applicantRole = person.EmployeeIds.FirstOrDefault(e => e.RoleType == EmployeeRoleType.Applicant && e.IsActive);
        if (applicantRole != null) applicantRole.IsActive = false;

        var resellerRole = new EmployeeId
        {
            Code = newCode,
            RoleType = EmployeeRoleType.Reseller,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
            PersonId = personId,
            ResidenceId = req.ResidenceId
        };

        _context.EmployeeIds.Add(resellerRole);
        await _context.SaveChangesAsync();

        return Ok(new { id = resellerRole.Id, code = newCode, role = "Reseller" });
    }

    /// <summary>
    /// Add Delivery role to a person (adds DEL-XXXX EmployeeId alongside any existing roles)
    /// </summary>
    [HttpPost("people/{personId}/assign-delivery")]
    public async Task<IActionResult> AssignDelivery(Guid personId)
    {
        var person = await _context.Persons
            .Include(p => p.EmployeeIds)
            .FirstOrDefaultAsync(p => p.Id == personId);
        if (person == null) return NotFound(new { error = "Person not found." });

        // Check if they already have an active Delivery role
        var existing = person.EmployeeIds
            .FirstOrDefault(e => e.RoleType == EmployeeRoleType.Delivery && e.IsActive);
        if (existing != null)
            return BadRequest(new { error = $"Already has Delivery role: {existing.Code}" });

        // Generate unique DEL code
        var count = await _context.EmployeeIds.CountAsync(e => e.RoleType == EmployeeRoleType.Delivery);
        var newCode = $"DEL-{(count + 1):D4}";

        // Deactivate the Applicant role if still active
        var applicantRole = person.EmployeeIds.FirstOrDefault(e => e.RoleType == EmployeeRoleType.Applicant && e.IsActive);
        if (applicantRole != null) applicantRole.IsActive = false;

        var deliveryRole = new EmployeeId
        {
            Code = newCode,
            RoleType = EmployeeRoleType.Delivery,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
            PersonId = personId,
            ResidenceId = null
        };

        _context.EmployeeIds.Add(deliveryRole);
        await _context.SaveChangesAsync();

        return Ok(new { id = deliveryRole.Id, code = newCode, role = "Delivery" });
    }

    /// <summary>
    /// Deactivate a role (e.g., terminate a reseller)
    /// </summary>
    [HttpPost("employees/{employeeId}/deactivate")]
    public async Task<IActionResult> DeactivateRole(Guid employeeId)
    {
        var emp = await _context.EmployeeIds.FindAsync(employeeId);
        if (emp == null) return NotFound();

        emp.IsActive = false;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"{emp.Code} deactivated." });
    }
}
