using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface IMilestoneService
{
    Task CheckAndTriggerAsync(Guid resellerEmployeeId, Guid productVariantId);
    Task<List<MilestoneAlert>> GetAlertsAsync(bool? paidOnly = null);
    Task<MilestoneAlert> MarkPaidAsync(Guid alertId, string? notes);
    Task<List<MilestoneConfig>> GetAllConfigsAsync();
    Task<MilestoneConfig> CreateConfigAsync(Guid productVariantId, int targetUnits, decimal bonusAmount, MilestoneTimeframe timeframe, Guid? adminId);
    Task DeleteConfigAsync(Guid id);
}

public class MilestoneService : IMilestoneService
{
    private readonly BakeryDbContext _context;
    private readonly INotificationService _notificationService;

    public MilestoneService(BakeryDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task CheckAndTriggerAsync(Guid resellerEmployeeId, Guid productVariantId)
    {
        // Find all active milestone configs for this variant
        var configs = await _context.MilestoneConfigs
            .Include(c => c.ProductVariant).ThenInclude(v => v.Product)
            .Where(c => c.ProductVariantId == productVariantId && c.IsActive)
            .ToListAsync();

        if (!configs.Any()) return;

        var reseller = await _context.EmployeeIds
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId);
        if (reseller == null) return;

        foreach (var config in configs)
        {
            var periodKey = GetPeriodKey(config.Timeframe);
            var (fromDate, toDate) = GetPeriodRange(config.Timeframe);

            // Sum units sold in period
            var unitsSold = await _context.ResellerSales
                .Where(s => s.ResellerEmployeeId == resellerEmployeeId
                         && s.ProductVariantId == productVariantId
                         && s.SaleDate >= fromDate
                         && s.SaleDate <= toDate)
                .SumAsync(s => (int?)s.Quantity) ?? 0;

            if (unitsSold < config.TargetUnits) continue;

            // Check if alert already exists for this period
            var existing = await _context.MilestoneAlerts
                .AnyAsync(a => a.MilestoneConfigId == config.Id
                            && a.ResellerEmployeeId == resellerEmployeeId
                            && a.PeriodKey == periodKey);
            if (existing) continue;

            // Create alert
            var alert = new MilestoneAlert
            {
                MilestoneConfigId = config.Id,
                ResellerEmployeeId = resellerEmployeeId,
                AchievedUnits = unitsSold,
                BonusOwed = config.BonusAmount,
                PeriodKey = periodKey,
                TriggeredAt = DateTime.UtcNow
            };
            _context.MilestoneAlerts.Add(alert);
            await _context.SaveChangesAsync();

            // Notify admin
            await _notificationService.NotifyMilestoneReachedAsync(
                reseller.Code,
                $"{reseller.Person.FirstName} {reseller.Person.LastName}",
                config.ProductVariant.Product?.Name ?? "Unknown",
                config.ProductVariant.SizeName,
                unitsSold,
                config.BonusAmount,
                periodKey
            );
        }
    }

    public async Task<List<MilestoneAlert>> GetAlertsAsync(bool? paidOnly = null)
    {
        var query = _context.MilestoneAlerts
            .Include(a => a.Reseller).ThenInclude(e => e.Person)
            .Include(a => a.MilestoneConfig).ThenInclude(c => c.ProductVariant).ThenInclude(v => v.Product)
            .AsQueryable();

        if (paidOnly.HasValue)
            query = paidOnly.Value ? query.Where(a => a.IsPaid) : query.Where(a => !a.IsPaid);

        return await query.OrderByDescending(a => a.TriggeredAt).ToListAsync();
    }

    public async Task<MilestoneAlert> MarkPaidAsync(Guid alertId, string? notes)
    {
        var alert = await _context.MilestoneAlerts.FindAsync(alertId);
        if (alert == null) throw new Exception("Alert not found.");

        alert.IsPaid = true;
        alert.PaidAt = DateTime.UtcNow;
        alert.AdminNotes = notes;
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task<List<MilestoneConfig>> GetAllConfigsAsync()
    {
        return await _context.MilestoneConfigs
            .Include(c => c.ProductVariant).ThenInclude(v => v.Product)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<MilestoneConfig> CreateConfigAsync(Guid productVariantId, int targetUnits, decimal bonusAmount, MilestoneTimeframe timeframe, Guid? adminId)
    {
        if (targetUnits <= 0) throw new Exception("Target units must be positive.");
        if (bonusAmount <= 0) throw new Exception("Bonus amount must be positive.");

        var variant = await _context.ProductVariants.FindAsync(productVariantId);
        if (variant == null) throw new Exception("Product variant not found.");

        var config = new MilestoneConfig
        {
            ProductVariantId = productVariantId,
            TargetUnits = targetUnits,
            BonusAmount = bonusAmount,
            Timeframe = timeframe,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByAdminId = adminId
        };

        _context.MilestoneConfigs.Add(config);
        await _context.SaveChangesAsync();
        return config;
    }

    public async Task DeleteConfigAsync(Guid id)
    {
        var config = await _context.MilestoneConfigs.FindAsync(id);
        if (config == null) throw new Exception("Config not found.");
        config.IsActive = false;
        await _context.SaveChangesAsync();
    }

    private static string GetPeriodKey(MilestoneTimeframe timeframe)
    {
        var now = DateTime.UtcNow;
        return timeframe switch
        {
            MilestoneTimeframe.Monthly => $"{now.Year}-{now.Month:D2}",
            MilestoneTimeframe.Weekly => $"{now.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(now):D2}",
            MilestoneTimeframe.Lifetime => "LIFETIME",
            _ => "UNKNOWN"
        };
    }

    private static (DateTime from, DateTime to) GetPeriodRange(MilestoneTimeframe timeframe)
    {
        var now = DateTime.UtcNow;
        return timeframe switch
        {
            MilestoneTimeframe.Monthly => (
                new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddSeconds(-1)
            ),
            MilestoneTimeframe.Weekly => (
                DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)),
                DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday ? -6 : 1)).AddDays(7).AddSeconds(-1)
            ),
            MilestoneTimeframe.Lifetime => (DateTime.MinValue, DateTime.MaxValue),
            _ => (DateTime.MinValue, DateTime.MaxValue)
        };
    }
}
