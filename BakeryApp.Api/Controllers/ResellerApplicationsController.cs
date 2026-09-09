using BakeryApp.Api.DTOs;
using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResellerApplicationsController : ControllerBase
{
    private readonly IResellerApplicationService _applicationService;

    public ResellerApplicationsController(IResellerApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitApplication([FromBody] SubmitApplicationRequest request)
    {
        var application = new ResellerApplication
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ResidenceName = request.ResidenceName,
            RoomNumber = request.RoomNumber,
            EstimatedResidencePopulation = request.EstimatedResidencePopulation,
            PreferredSellingArea = request.PreferredSellingArea,
            PreviousSalesExperience = request.PreviousSalesExperience,
            Availability = request.Availability,
            ExpectedTimeAtResidence = request.ExpectedTimeAtResidence,
            AdditionalInfo = request.AdditionalInfo,
            Status = ApplicationStatus.PENDING,
            SubmittedAt = DateTime.UtcNow
        };

        try
        {
            var saved = await _applicationService.SubmitApplicationAsync(application);
            return Ok(new { id = saved.Id, message = "Application submitted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetApplications([FromQuery] ApplicationStatus? status)
    {
        var apps = await _applicationService.GetApplicationsAsync(status);
        var response = apps.Select(a => new ApplicationResponseDto
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            PhoneNumber = a.PhoneNumber,
            ResidenceName = a.ResidenceName,
            Status = a.Status,
            SubmittedAt = a.SubmittedAt,
            ReviewedAt = a.ReviewedAt,
            AdminNotes = a.AdminNotes,
            EmployeeIdCode = a.ApprovedEmployee?.Code
        });
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetApplication(Guid id)
    {
        var app = await _applicationService.GetApplicationByIdAsync(id);
        if (app == null) return NotFound();
        return Ok(new ApplicationResponseDto
        {
            Id = app.Id,
            FirstName = app.FirstName,
            LastName = app.LastName,
            Email = app.Email,
            PhoneNumber = app.PhoneNumber,
            ResidenceName = app.ResidenceName,
            Status = app.Status,
            SubmittedAt = app.SubmittedAt,
            ReviewedAt = app.ReviewedAt,
            AdminNotes = app.AdminNotes,
            EmployeeIdCode = app.ApprovedEmployee?.Code
        });
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReviewApplication(Guid id, [FromBody] ReviewApplicationRequest request)
    {
        var adminEmployeeCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(adminEmployeeCode))
            return Unauthorized("Admin employee code not found in token.");

        try
        {
            var updated = await _applicationService.ReviewApplicationAsync(
                id,
                request.Status,
                request.ResidenceId,
                request.AdminNotes,
                adminEmployeeCode
            );
            return Ok(new { id = updated.Id, status = updated.Status.ToString(), employeeId = updated.ApprovedEmployee?.Code });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
