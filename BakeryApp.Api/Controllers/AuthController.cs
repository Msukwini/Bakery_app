using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BakeryApp.Api.Controllers;

public record LoginRequest(string Email, string Code);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BakeryDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(BakeryDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var employee = await _dbContext.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Code == request.Code && e.Person.Email == request.Email);

        if (employee == null || !employee.IsActive)
        {
            return Unauthorized(new { Message = "Invalid credentials or inactive employee record." });
        }

        var token = GenerateJwtToken(employee.Person.Email, employee.RoleType.ToString(), employee.Id);

        return Ok(new
        {
            Token = token,
            EmployeeId = employee.Id,
            Role = employee.RoleType.ToString()
        });
    }

    private string GenerateJwtToken(string email, string role, Guid employeeId)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] 
            ?? "YourSuperSecretKeyHere_MustBeAtLeast32BytesLong!");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employeeId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(secretKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "BakeryAppApi",
            audience: jwtSettings["Audience"] ?? "BakeryAppClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}