using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface IResellerApplicationService
{
    Task<ResellerApplication> SubmitApplicationAsync(ResellerApplication application);
    Task<ResellerApplication?> GetApplicationByIdAsync(Guid id);
    Task<List<ResellerApplication>> GetApplicationsAsync(ApplicationStatus? status = null);
    Task<ResellerApplication> ReviewApplicationAsync(Guid applicationId, ApplicationStatus status, Guid? residenceId, string? adminNotes, string adminEmployeeCode);
}

public class ResellerApplicationService : IResellerApplicationService
{
    private readonly BakeryDbContext _context;
    private readonly INotificationService _notificationService;

    public ResellerApplicationService(BakeryDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<ResellerApplication> SubmitApplicationAsync(ResellerApplication application)
    {
        var existingPerson = await _context.Persons.AnyAsync(p => p.Email == application.Email);
        if (existingPerson)
            throw new Exception("An account with this email already exists. Please log in.");

        var existingApp = await _context.ResellerApplications.AnyAsync(a => a.Email == application.Email && a.Status == ApplicationStatus.PENDING);
        if (existingApp)
            throw new Exception("You already have a pending application.");

        _context.ResellerApplications.Add(application);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyResellerApplicationSubmittedAsync(application);
        return application;
    }

    public async Task<ResellerApplication?> GetApplicationByIdAsync(Guid id)
    {
        return await _context.ResellerApplications
            .Include(a => a.Residence)
            .Include(a => a.ApprovedEmployee)
            .Include(a => a.ReviewedByAdmin)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<ResellerApplication>> GetApplicationsAsync(ApplicationStatus? status = null)
    {
        var query = _context.ResellerApplications.AsQueryable();
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        return await query
            .Include(a => a.Residence)
            .Include(a => a.ApprovedEmployee)
            .Include(a => a.ReviewedByAdmin)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();
    }

    public async Task<ResellerApplication> ReviewApplicationAsync(Guid applicationId, ApplicationStatus status, Guid? residenceId, string? adminNotes, string adminEmployeeCode)
    {
        var application = await GetApplicationByIdAsync(applicationId);
        if (application == null)
            throw new Exception("Application not found.");

        if (application.Status != ApplicationStatus.PENDING && application.Status != ApplicationStatus.UNDER_REVIEW)
            throw new Exception("Application is already reviewed.");

        var adminEmployee = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == adminEmployeeCode && e.RoleType == EmployeeRoleType.Admin);
        if (adminEmployee == null)
            throw new Exception("Admin not found.");

        if (status == ApplicationStatus.APPROVED)
        {
            if (!residenceId.HasValue)
                throw new Exception("ResidenceId is required for approval.");

            var residence = await _context.Residences.FindAsync(residenceId.Value);
            if (residence == null)
                throw new Exception("Residence not found.");

            var activeResellersCount = await _context.EmployeeIds
                .CountAsync(e => e.ResidenceId == residenceId.Value && e.RoleType == EmployeeRoleType.Reseller && e.IsActive);

            if (activeResellersCount >= residence.MaxResellerCapacity)
                throw new Exception($"Residence capacity reached (max {residence.MaxResellerCapacity}). Override not implemented yet.");

            var person = await _context.Persons.FirstOrDefaultAsync(p => p.Email == application.Email);
            if (person == null)
            {
                person = new Person
                {
                    FirstName = application.FirstName,
                    LastName = application.LastName,
                    Email = application.Email,
                    PhoneNumber = application.PhoneNumber,
                    PasswordHash = "" // They will set password later via "forgot password" or registration
                };
                _context.Persons.Add(person);
                await _context.SaveChangesAsync();
            }

            var count = await _context.EmployeeIds.CountAsync(e => e.RoleType == EmployeeRoleType.Reseller) + 1;
            var code = $"RES-{count:D4}";

            var employee = new EmployeeId
            {
                Code = code,
                RoleType = EmployeeRoleType.Reseller,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                PersonId = person.Id,
                ResidenceId = residenceId.Value
            };

            _context.EmployeeIds.Add(employee);
            await _context.SaveChangesAsync();

            application.ApprovedEmployeeId = employee.Id;
            application.ResidenceId = residenceId.Value;
        }
        else
        {
            if (residenceId.HasValue)
                throw new Exception("ResidenceId should not be provided for non-approval.");
        }

        application.Status = status;
        application.ReviewedAt = DateTime.UtcNow;
        application.AdminNotes = adminNotes;
        application.ReviewedByAdminId = adminEmployee.Id;

        await _context.SaveChangesAsync();
        return application;
    }
}
