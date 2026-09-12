using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public TeamController(BakeryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// For delivery people: get all resellers they deliver to.
    /// For resellers: get the delivery person assigned to them.
    /// </summary>
    [HttpGet("my-team")]
    public async Task<IActionResult> GetMyTeam()
    {
        var code = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(code)) return Unauthorized();

        var me = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Code == code);
        if (me == null) return Unauthorized();

        if (me.RoleType == EmployeeRoleType.Delivery)
        {
            // Find all active permanent assignments where I'm the delivery person
            var assignments = await _context.DeliveryAssignments
                .Include(a => a.Reseller).ThenInclude(e => e.Person)
                .Include(a => a.Reseller).ThenInclude(e => e.Residence)
                .Where(a => a.PermanentDeliveryEmployeeId == me.Id
                         && a.Type == AssignmentType.PERMANENT
                         && (a.EndDate == null || a.EndDate >= DateTime.UtcNow.Date))
                .Select(a => new
                {
                    ResellerEmployeeId = a.Reseller.Id,
                    ResellerCode = a.Reseller.Code,
                    PersonId = a.Reseller.Person.Id,
                    FirstName = a.Reseller.Person.FirstName,
                    LastName = a.Reseller.Person.LastName,
                    PhoneNumber = a.Reseller.Person.PhoneNumber,
                    Email = a.Reseller.Person.Email,
                    UniversityName = a.Reseller.Person.UniversityName,
                    HasProfilePicture = !string.IsNullOrEmpty(a.Reseller.Person.ProfilePicturePath),
                    ResidenceName = a.Reseller.Residence != null ? a.Reseller.Residence.Name : null,
                    AssignmentStartDate = a.StartDate,
                    AssignmentType = a.Type.ToString()
                })
                .ToListAsync();

            return Ok(new { role = "Delivery", team = assignments });
        }
        else if (me.RoleType == EmployeeRoleType.Reseller)
        {
            // Find the permanent delivery employee for me
            var assignment = await _context.DeliveryAssignments
                .Include(a => a.PermanentDeliveryEmployee).ThenInclude(e => e.Person)
                .Include(a => a.ActualDeliveryEmployee).ThenInclude(e => e!.Person)
                .Where(a => a.ResellerEmployeeId == me.Id
                         && a.Type == AssignmentType.PERMANENT
                         && (a.EndDate == null || a.EndDate >= DateTime.UtcNow.Date))
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefaultAsync();

            if (assignment == null)
                return Ok(new { role = "Reseller", deliveryPerson = (object?)null });

            // Prefer actual (temporary) over permanent if set
            var actualEmp = assignment.ActualDeliveryEmployee ?? assignment.PermanentDeliveryEmployee;

            return Ok(new
            {
                role = "Reseller",
                deliveryPerson = new
                {
                    EmployeeId = actualEmp.Id,
                    EmployeeCode = actualEmp.Code,
                    PersonId = actualEmp.Person.Id,
                    FirstName = actualEmp.Person.FirstName,
                    LastName = actualEmp.Person.LastName,
                    PhoneNumber = actualEmp.Person.PhoneNumber,
                    HasProfilePicture = !string.IsNullOrEmpty(actualEmp.Person.ProfilePicturePath),
                    IsTemporary = assignment.ActualDeliveryEmployeeId != null && assignment.ActualDeliveryEmployeeId != assignment.PermanentDeliveryEmployeeId
                }
            });
        }

        return Ok(new { role = me.RoleType.ToString(), team = Array.Empty<object>() });
    }
}
