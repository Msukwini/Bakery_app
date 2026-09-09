using BakeryApp.Api.DTOs;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly BakeryDbContext _context;

    public AuthController(IAuthService authService, BakeryDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Validate that a ResidenceId is provided if role is Reseller
        if (request.Role.ToLower() == "reseller" && request.ResidenceId == null)
        {
            return BadRequest("ResidenceId is required for Reseller registration.");
        }

        // Parse the role
        if (!Enum.TryParse<EmployeeRoleType>(request.Role, true, out var role))
        {
            return BadRequest("Invalid role. Valid roles are: Admin, Reseller.");
        }

        // For simplicity, we restrict automatic Admin registration. Admins must be created manually or via seed.
////        if (role == EmployeeRoleType.Admin)
////        {
////            // Check if any Admin already exists
////            var adminExists = await _context.EmployeeIds.AnyAsync(e => e.RoleType == EmployeeRoleType.Admin);
////            if (adminExists) return Forbid("Admin registration is restricted. Use the seeded admin account.");
////        }

        try
        {
            var employee = await _authService.RegisterUserAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Password,
                role,
                request.ResidenceId
            );

            // Generate token for immediate login
            var token = await _authService.GenerateJwtToken(employee);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = request.Email,
                Role = role.ToString(),
                EmployeeId = employee.Code
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var employee = await _authService.ValidateUserCredentials(request.Email, request.Password);
        if (employee == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = await _authService.GenerateJwtToken(employee);
        var person = await _context.Persons.FindAsync(employee.PersonId);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = person?.Email ?? "",
            Role = employee.RoleType.ToString(),
            EmployeeId = employee.Code
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("EmployeeCode")?.Value;
        if (userId == null) return Unauthorized();

        var employee = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Code == userId);

        if (employee == null) return NotFound();

        return Ok(new
        {
            employee.Code,
            employee.RoleType,
            employee.IsActive,
            Person = new
            {
                employee.Person.FirstName,
                employee.Person.LastName,
                employee.Person.Email,
                employee.Person.PhoneNumber
            }
        });
    }
}
