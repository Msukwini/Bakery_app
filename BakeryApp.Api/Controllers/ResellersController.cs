using BakeryApp.Api.DTOs;
using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResellersController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public ResellersController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResellerResponseDto>>> GetAll()
    {
        var resellers = await _context.EmployeeIds
            .Include(e => e.Person)
            .Include(e => e.Residence)
            .Where(e => e.RoleType == EmployeeRoleType.Reseller)
            .Select(e => new ResellerResponseDto(
                e.Id,
                e.PersonId,
                e.Code,
                e.Person.FirstName,
                e.Person.LastName,
                e.Person.Email,
                e.Person.PhoneNumber,
                e.IsActive,
                e.ResidenceId,
                e.Residence != null ? e.Residence.Name : null
            ))
            .ToListAsync();

        return Ok(resellers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResellerResponseDto>> GetById(Guid id)
    {
        var reseller = await _context.EmployeeIds
            .Include(e => e.Person)
            .Include(e => e.Residence)
            .Where(e => e.Id == id && e.RoleType == EmployeeRoleType.Reseller)
            .Select(e => new ResellerResponseDto(
                e.Id,
                e.PersonId,
                e.Code,
                e.Person.FirstName,
                e.Person.LastName,
                e.Person.Email,
                e.Person.PhoneNumber,
                e.IsActive,
                e.ResidenceId,
                e.Residence != null ? e.Residence.Name : null
            ))
            .FirstOrDefaultAsync();

        if (reseller == null) return NotFound();

        return Ok(reseller);
    }

    [HttpPost]
    public async Task<ActionResult<ResellerResponseDto>> Create(CreateResellerDto dto)
    {
        if (await _context.EmployeeIds.AnyAsync(e => e.Code == dto.Code))
        {
            return BadRequest($"Employee code '{dto.Code}' is already in use.");
        }

        if (dto.ResidenceId.HasValue)
        {
            var residence = await _context.Residences
                .Include(r => r.AssignedResellers)
                .FirstOrDefaultAsync(r => r.Id == dto.ResidenceId.Value);

            if (residence == null) return BadRequest("Target residence does not exist.");

            if (residence.AssignedResellers.Count >= residence.MaxResellerCapacity)
            {
                return BadRequest($"Residence '{residence.Name}' has reached maximum reseller capacity ({residence.MaxResellerCapacity}).");
            }
        }

        var person = new Person
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var employeeId = new EmployeeId
        {
            Code = dto.Code,
            RoleType = EmployeeRoleType.Reseller,
            ResidenceId = dto.ResidenceId,
            Person = person
        };

        _context.Persons.Add(person);
        _context.EmployeeIds.Add(employeeId);
        await _context.SaveChangesAsync();

        var response = new ResellerResponseDto(
            employeeId.Id,
            person.Id,
            employeeId.Code,
            person.FirstName,
            person.LastName,
            person.Email,
            person.PhoneNumber,
            employeeId.IsActive,
            employeeId.ResidenceId,
            employeeId.Residence?.Name
        );

        return CreatedAtAction(nameof(GetById), new { id = employeeId.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateResellerDto dto)
    {
        var employee = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == id && e.RoleType == EmployeeRoleType.Reseller);

        if (employee == null) return NotFound();

        employee.Person.FirstName = dto.FirstName;
        employee.Person.LastName = dto.LastName;
        employee.Person.Email = dto.Email;
        employee.Person.PhoneNumber = dto.PhoneNumber;
        employee.IsActive = dto.IsActive;
        employee.ResidenceId = dto.ResidenceId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == id && e.RoleType == EmployeeRoleType.Reseller);

        if (employee == null) return NotFound();

        _context.EmployeeIds.Remove(employee);
        _context.Persons.Remove(employee.Person);

        await _context.SaveChangesAsync();
        return NoContent();
    }
}