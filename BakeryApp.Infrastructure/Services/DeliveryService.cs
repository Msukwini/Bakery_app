using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface IDeliveryService
{
    Task<DeliveryAssignment> AssignResellerAsync(Guid resellerEmployeeId, Guid deliveryEmployeeId, Guid? permanentDeliveryEmployeeId, AssignmentType type, DateTime startDate, DateTime? endDate, string? reason, Guid? createdByAdminId);
    Task<List<DeliveryAssignment>> GetActiveAssignmentsForResellerAsync(Guid resellerEmployeeId, DateTime? date = null);
    Task<List<DeliveryAssignment>> GetAssignmentsForDeliveryEmployeeAsync(Guid deliveryEmployeeId, DateTime? date = null);
    Task<DeliveryAssignment> UpdateAssignmentAsync(Guid assignmentId, DateTime? endDate, string? reason);
    Task<List<DeliveryAssignment>> GetAssignmentHistoryForResellerAsync(Guid resellerEmployeeId);
    Task<List<DeliveryAssignment>> GetAllAssignmentsAsync(bool activeOnly);
    Task<DeliveryEarning> RecordDeliveryCompletionAsync(Guid deliveryEmployeeId, DateTime completionDate, decimal? ratePerDay = null);
    Task<List<DeliveryEarning>> GetEarningsLedgerAsync(Guid deliveryEmployeeId);
    Task<decimal> GetOutstandingEarningsAsync(Guid deliveryEmployeeId);
    Task<List<DeliveryEarning>> ProcessDeliveryPayoutAsync(Guid deliveryEmployeeId, decimal amount, string? notes = null);
}

public class DeliveryService : IDeliveryService
{
    private readonly BakeryDbContext _context;
    private const decimal DefaultDailyRate = 150.0m;

    public DeliveryService(BakeryDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryAssignment> AssignResellerAsync(Guid resellerEmployeeId, Guid deliveryEmployeeId, Guid? permanentDeliveryEmployeeId, AssignmentType type, DateTime startDate, DateTime? endDate, string? reason, Guid? createdByAdminId)
    {
        var reseller = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller && e.IsActive);
        if (reseller == null) throw new Exception("Reseller not found or inactive.");

        var deliveryEmp = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == deliveryEmployeeId && e.RoleType == EmployeeRoleType.Delivery && e.IsActive);
        if (deliveryEmp == null) throw new Exception("Delivery employee not found or inactive.");

        if (type == AssignmentType.PERMANENT)
        {
            // End any existing permanent assignment
            var activePermanent = await _context.DeliveryAssignments
                .FirstOrDefaultAsync(a => a.ResellerEmployeeId == resellerEmployeeId && a.Type == AssignmentType.PERMANENT && a.EndDate == null);
            if (activePermanent != null)
            {
                activePermanent.EndDate = startDate.AddDays(-1);
                activePermanent.Reason = "Reassigned permanently";
                _context.DeliveryAssignments.Update(activePermanent);
            }

            var assignment = new DeliveryAssignment
            {
                ResellerEmployeeId = resellerEmployeeId,
                Reseller = reseller,
                PermanentDeliveryEmployeeId = deliveryEmployeeId,
                PermanentDeliveryEmployee = deliveryEmp,
                ActualDeliveryEmployeeId = deliveryEmployeeId,
                ActualDeliveryEmployee = deliveryEmp,
                Type = AssignmentType.PERMANENT,
                StartDate = startDate,
                EndDate = endDate,
                Reason = reason ?? "",
                CreatedByAdminId = createdByAdminId
            };

            _context.DeliveryAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }
        else // TEMPORARY
        {
            // Find the current permanent delivery employee (default if not specified)
            Guid permId = permanentDeliveryEmployeeId ?? Guid.Empty;
            if (permId == Guid.Empty)
            {
                var currentPerm = await _context.DeliveryAssignments
                    .Where(a => a.ResellerEmployeeId == resellerEmployeeId && a.Type == AssignmentType.PERMANENT && a.EndDate == null)
                    .OrderByDescending(a => a.StartDate)
                    .FirstOrDefaultAsync();
                if (currentPerm == null)
                    throw new Exception("This reseller has no permanent delivery employee. Assign one permanently first.");
                permId = currentPerm.PermanentDeliveryEmployeeId;
            }

            var permEmp = await _context.EmployeeIds.FindAsync(permId);
            if (permEmp == null) throw new Exception("Permanent delivery employee not found.");

            var assignment = new DeliveryAssignment
            {
                ResellerEmployeeId = resellerEmployeeId,
                Reseller = reseller,
                PermanentDeliveryEmployeeId = permId,
                PermanentDeliveryEmployee = permEmp,
                ActualDeliveryEmployeeId = deliveryEmployeeId,
                ActualDeliveryEmployee = deliveryEmp,
                Type = AssignmentType.TEMPORARY,
                StartDate = startDate,
                EndDate = endDate,
                Reason = reason ?? "Temporary coverage",
                CreatedByAdminId = createdByAdminId
            };

            _context.DeliveryAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }
    }

    public async Task<List<DeliveryAssignment>> GetActiveAssignmentsForResellerAsync(Guid resellerEmployeeId, DateTime? date = null)
    {
        var queryDate = date ?? DateTime.UtcNow.Date;
        return await _context.DeliveryAssignments
            .Where(a => a.ResellerEmployeeId == resellerEmployeeId
                        && a.StartDate <= queryDate
                        && (a.EndDate == null || a.EndDate >= queryDate))
            .Include(a => a.PermanentDeliveryEmployee)
            .Include(a => a.ActualDeliveryEmployee)
            .ToListAsync();
    }

    public async Task<List<DeliveryAssignment>> GetAssignmentsForDeliveryEmployeeAsync(Guid deliveryEmployeeId, DateTime? date = null)
    {
        var queryDate = date ?? DateTime.UtcNow.Date;
        return await _context.DeliveryAssignments
            .Where(a => (a.PermanentDeliveryEmployeeId == deliveryEmployeeId || a.ActualDeliveryEmployeeId == deliveryEmployeeId)
                        && a.StartDate <= queryDate
                        && (a.EndDate == null || a.EndDate >= queryDate))
            .Include(a => a.Reseller)
            .ToListAsync();
    }

    public async Task<DeliveryAssignment> UpdateAssignmentAsync(Guid assignmentId, DateTime? endDate, string? reason)
    {
        var assignment = await _context.DeliveryAssignments.FindAsync(assignmentId);
        if (assignment == null) throw new Exception("Assignment not found.");
        if (endDate.HasValue && endDate.Value < assignment.StartDate)
            throw new Exception("End date cannot be before start date.");

        assignment.EndDate = endDate;
        if (!string.IsNullOrEmpty(reason)) assignment.Reason = reason;
        _context.DeliveryAssignments.Update(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<List<DeliveryAssignment>> GetAssignmentHistoryForResellerAsync(Guid resellerEmployeeId)
    {
        return await _context.DeliveryAssignments
            .Where(a => a.ResellerEmployeeId == resellerEmployeeId)
            .OrderByDescending(a => a.StartDate)
            .Include(a => a.PermanentDeliveryEmployee)
            .Include(a => a.ActualDeliveryEmployee)
            .ToListAsync();
    }

    public async Task<List<DeliveryAssignment>> GetAllAssignmentsAsync(bool activeOnly)
    {
        var query = _context.DeliveryAssignments
            .Include(a => a.Reseller).ThenInclude(e => e.Person)
            .Include(a => a.PermanentDeliveryEmployee).ThenInclude(e => e.Person)
            .Include(a => a.ActualDeliveryEmployee).ThenInclude(e => e.Person)
            .AsQueryable();

        if (activeOnly)
        {
            var today = DateTime.UtcNow.Date;
            query = query.Where(a => a.StartDate <= today && (a.EndDate == null || a.EndDate >= today));
        }

        return await query.OrderByDescending(a => a.StartDate).ToListAsync();
    }

    public async Task<DeliveryEarning> RecordDeliveryCompletionAsync(Guid deliveryEmployeeId, DateTime completionDate, decimal? ratePerDay = null)
    {
        var rate = ratePerDay ?? DefaultDailyRate;
        var employee = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == deliveryEmployeeId && e.RoleType == EmployeeRoleType.Delivery && e.IsActive);
        if (employee == null) throw new Exception("Delivery employee not found or inactive.");

        var existing = await _context.DeliveryEarnings
            .FirstOrDefaultAsync(e => e.DeliveryEmployeeId == deliveryEmployeeId && e.EarningDate.Date == completionDate.Date);
        if (existing != null)
            throw new Exception($"Earnings already recorded for {completionDate.ToShortDateString()}.");

        var earning = new DeliveryEarning
        {
            DeliveryEmployeeId = deliveryEmployeeId,
            DeliveryEmployee = employee,
            DeliveryAssignmentId = null,
            AmountEarned = rate,
            AmountPaid = 0,
            IsSettled = false,
            EarningDate = completionDate
        };

        _context.DeliveryEarnings.Add(earning);
        await _context.SaveChangesAsync();
        return earning;
    }

    public async Task<List<DeliveryEarning>> GetEarningsLedgerAsync(Guid deliveryEmployeeId)
    {
        return await _context.DeliveryEarnings
            .Where(e => e.DeliveryEmployeeId == deliveryEmployeeId)
            .Include(e => e.DeliveryEmployee)
            .OrderByDescending(e => e.EarningDate)
            .ToListAsync();
    }

    public async Task<decimal> GetOutstandingEarningsAsync(Guid deliveryEmployeeId)
    {
        var earnings = await _context.DeliveryEarnings
            .Where(e => e.DeliveryEmployeeId == deliveryEmployeeId && !e.IsSettled)
            .ToListAsync();
        return earnings.Sum(e => e.AmountEarned - e.AmountPaid);
    }

    public async Task<List<DeliveryEarning>> ProcessDeliveryPayoutAsync(Guid deliveryEmployeeId, decimal amount, string? notes = null)
    {
        if (amount <= 0) throw new Exception("Payout amount must be positive.");

        var employee = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == deliveryEmployeeId && e.RoleType == EmployeeRoleType.Delivery && e.IsActive);
        if (employee == null) throw new Exception("Delivery employee not found or inactive.");

        var unsettledEntries = await _context.DeliveryEarnings
            .Where(e => e.DeliveryEmployeeId == deliveryEmployeeId && !e.IsSettled)
            .OrderBy(e => e.EarningDate)
            .ToListAsync();

        if (!unsettledEntries.Any()) throw new Exception("No outstanding earnings to pay.");

        var totalOutstanding = unsettledEntries.Sum(e => e.AmountEarned - e.AmountPaid);
        if (amount > totalOutstanding)
            throw new Exception($"Amount exceeds outstanding balance. Outstanding: {totalOutstanding}");

        var remaining = amount;
        var updatedEntries = new List<DeliveryEarning>();

        foreach (var entry in unsettledEntries)
        {
            if (remaining <= 0) break;
            var due = entry.AmountEarned - entry.AmountPaid;
            if (due <= 0) continue;

            if (remaining >= due)
            {
                entry.AmountPaid += due;
                entry.IsSettled = true;
                remaining -= due;
            }
            else
            {
                entry.AmountPaid += remaining;
                remaining = 0;
            }
            updatedEntries.Add(entry);
        }

        await _context.SaveChangesAsync();
        return updatedEntries;
    }
}
