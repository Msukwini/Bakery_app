using BakeryApp.Api.DTOs;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly BakeryDbContext _context;
    private readonly string _uploadDir = "/opt/bakery/uploads/profile-pictures";
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public ProfileController(BakeryDbContext context)
    {
        _context = context;
        if (!Directory.Exists(_uploadDir))
            Directory.CreateDirectory(_uploadDir);
    }

    private async Task<Guid?> GetCurrentPersonIdAsync()
    {
        var code = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(code)) return null;
        var emp = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == code);
        return emp?.PersonId;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null) return Unauthorized();

        var person = await _context.Persons
            .Include(p => p.EmployeeIds)
            .FirstOrDefaultAsync(p => p.Id == personId);
        if (person == null) return NotFound();

        return Ok(new ProfileResponseDto
        {
            PersonId = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email,
            PhoneNumber = person.PhoneNumber,
            UniversityName = person.UniversityName,
            StudentEmail = person.StudentEmail,
            HasProfilePicture = !string.IsNullOrEmpty(person.ProfilePicturePath),
            Roles = person.EmployeeIds
                .Where(e => e.IsActive)
                .Select(e => new RoleInfo { Code = e.Code, Role = e.RoleType.ToString() })
                .ToList()
        });
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        var personId = await GetCurrentPersonIdAsync();
        if (personId == null) return Unauthorized();

        var person = await _context.Persons.FindAsync(personId);
        if (person == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) person.FirstName = dto.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(dto.LastName)) person.LastName = dto.LastName.Trim();
        if (dto.PhoneNumber != null) person.PhoneNumber = dto.PhoneNumber.Trim();
        if (dto.UniversityName != null) person.UniversityName = dto.UniversityName.Trim();
        if (dto.StudentEmail != null) person.StudentEmail = dto.StudentEmail.Trim();

        await _context.SaveChangesAsync();
        return Ok(new { message = "Profile updated." });
    }

    [HttpPost("picture")]
    [Authorize]
    public async Task<IActionResult> UploadPicture(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "No file uploaded." });
        if (file.Length > MaxFileSizeBytes) return BadRequest(new { error = "File too large (max 5 MB)." });

        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { error = "Only JPEG, PNG, or WebP images allowed." });

        var personId = await GetCurrentPersonIdAsync();
        if (personId == null) return Unauthorized();

        var person = await _context.Persons.FindAsync(personId);
        if (person == null) return NotFound();

        // Delete old picture
        if (!string.IsNullOrEmpty(person.ProfilePicturePath))
        {
            var oldPath = Path.Combine(_uploadDir, person.ProfilePicturePath);
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

        // Save new picture
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";
        var fileName = $"{personId}{ext}";
        var fullPath = Path.Combine(_uploadDir, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        person.ProfilePicturePath = fileName;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Picture uploaded.", path = fileName });
    }

    [HttpGet("picture/{personId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPicture(Guid personId)
    {
        var person = await _context.Persons.FindAsync(personId);
        if (person == null || string.IsNullOrEmpty(person.ProfilePicturePath))
            return NotFound();

        var fullPath = Path.Combine(_uploadDir, person.ProfilePicturePath);
        if (!System.IO.File.Exists(fullPath)) return NotFound();

        var ext = Path.GetExtension(fullPath).ToLower();
        var contentType = ext switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        Response.Headers.Append("Cache-Control", "public, max-age=3600");
        return File(bytes, contentType);
    }

    /// <summary>
    /// Public-safe profile — first name, last name, phone, picture flag only
    /// </summary>
    [HttpGet("{personId}/public")]
    [Authorize]
    public async Task<IActionResult> GetPublicProfile(Guid personId)
    {
        var person = await _context.Persons.FindAsync(personId);
        if (person == null) return NotFound();

        return Ok(new PublicProfileDto
        {
            PersonId = person.Id,
            FirstName = person.FirstName,
            LastName = person.LastName,
            PhoneNumber = person.PhoneNumber,
            HasProfilePicture = !string.IsNullOrEmpty(person.ProfilePicturePath)
        });
    }
}
