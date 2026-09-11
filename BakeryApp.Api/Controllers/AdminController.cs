using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public AdminController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet("resellers")]
    public async Task<IActionResult> ListResellers()
    {
        var resellers = await _context.EmployeeIds
            .Include(e => e.Person)
            .Include(e => e.Residence)
            .Where(e => e.RoleType == EmployeeRoleType.Reseller)
            .OrderBy(e => e.Code)
            .Select(e => new
            {
                e.Id,
                e.Code,
                e.IsActive,
                Name = e.Person.FirstName + " " + e.Person.LastName,
                Email = e.Person.Email,
                Phone = e.Person.PhoneNumber,
                ResidenceName = e.Residence != null ? e.Residence.Name : null
            })
            .ToListAsync();

        return Ok(resellers);
    }

    [HttpGet("delivery-employees")]
    public async Task<IActionResult> ListDeliveryEmployees()
    {
        var employees = await _context.EmployeeIds
            .Include(e => e.Person)
            .Where(e => e.RoleType == EmployeeRoleType.Delivery)
            .OrderBy(e => e.Code)
            .Select(e => new
            {
                e.Id,
                e.Code,
                e.IsActive,
                Name = e.Person.FirstName + " " + e.Person.LastName,
                Email = e.Person.Email,
                Phone = e.Person.PhoneNumber
            })
            .ToListAsync();

        return Ok(employees);
    }
}
