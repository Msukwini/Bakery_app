using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace BakeryApp.Infrastructure.Services;

public interface IAuthService
{
    Task<string> GenerateJwtToken(EmployeeId employee);
    Task<EmployeeId?> ValidateUserCredentials(string email, string password);
    Task<EmployeeId> RegisterUserAsync(string firstName, string lastName, string email, string phone, string password, EmployeeRoleType role, Guid? residenceId);
    Task<bool> VerifySetupTokenAsync(string token);
    Task SetPasswordAsync(string token, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly BakeryDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(BakeryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<EmployeeId?> ValidateUserCredentials(string email, string password)
    {
        var person = await _context.Persons
            .Include(p => p.EmployeeIds)
            .FirstOrDefaultAsync(p => p.Email == email);

        if (person == null) return null;
        if (string.IsNullOrEmpty(person.PasswordHash)) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, person.PasswordHash)) return null;

        return person.EmployeeIds.FirstOrDefault(e => e.IsActive);
    }

    public async Task<string> GenerateJwtToken(EmployeeId employee)
    {
        var person = await _context.Persons.FindAsync(employee.PersonId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Email, person?.Email ?? ""),
            new Claim(ClaimTypes.Name, $"{person?.FirstName} {person?.LastName}".Trim()),
            new Claim(ClaimTypes.Role, employee.RoleType.ToString()),
            new Claim("EmployeeCode", employee.Code)
        };

        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]
            ?? "YourSuperSecretKeyHere_MustBeAtLeast32BytesLong!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "BakeryAppApi",
            audience: jwtSettings["Audience"] ?? "BakeryAppClients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<EmployeeId> RegisterUserAsync(string firstName, string lastName, string email, string phone, string password, EmployeeRoleType role, Guid? residenceId)
    {
        var existing = await _context.Persons.AnyAsync(p => p.Email == email);
        if (existing) throw new Exception("Email already registered.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var person = new Person
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phone,
            PasswordHash = passwordHash,
            HasSetPassword = true
        };

        var prefix = role switch
        {
            EmployeeRoleType.Admin => "ADM",
            EmployeeRoleType.Reseller => "RES",
            EmployeeRoleType.Delivery => "DEL",
            EmployeeRoleType.Applicant => "APP",
            _ => "UNK"
        };

        var count = await _context.EmployeeIds.CountAsync(e => e.RoleType == role) + 1;
        var code = $"{prefix}-{count:D4}";

        var employee = new EmployeeId
        {
            Code = code,
            RoleType = role,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
            Person = person,
            ResidenceId = residenceId
        };

        _context.Persons.Add(person);
        _context.EmployeeIds.Add(employee);
        await _context.SaveChangesAsync();

        return employee;
    }

    public async Task<bool> VerifySetupTokenAsync(string token)
    {
        var person = await _context.Persons
            .FirstOrDefaultAsync(p => p.PasswordSetupToken == token);
        if (person == null) return false;
        if (person.PasswordSetupTokenExpiry == null) return false;
        if (person.PasswordSetupTokenExpiry < DateTime.UtcNow) return false;
        return true;
    }

    public async Task SetPasswordAsync(string token, string newPassword)
    {
        var person = await _context.Persons
            .FirstOrDefaultAsync(p => p.PasswordSetupToken == token);
        if (person == null) throw new Exception("Invalid or expired token.");
        if (person.PasswordSetupTokenExpiry == null || person.PasswordSetupTokenExpiry < DateTime.UtcNow)
            throw new Exception("Token has expired.");

        person.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        person.PasswordSetupToken = null;
        person.PasswordSetupTokenExpiry = null;
        person.HasSetPassword = true;

        await _context.SaveChangesAsync();
    }
}
