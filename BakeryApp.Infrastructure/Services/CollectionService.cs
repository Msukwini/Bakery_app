using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface ICollectionService
{
    Task<CashCollection> RecordCollectionAsync(Guid deliveryEmployeeId, DateTime collectionDate, decimal collectedAmount, VarianceReason varianceReason, string? varianceNotes);
    Task<CashCollection?> GetCollectionAsync(Guid id);
    Task<List<CashCollection>> GetCollectionsAsync(Guid? deliveryEmployeeId, CollectionStatus? status);
    Task<CashCollection> ReconcileCollectionAsync(Guid id, string? adminNotes, string adminCode);

    Task<Deposit> SubmitDepositAsync(Guid deliveryEmployeeId, Guid? cashCollectionId, decimal amount, DateTime depositDate, string? bankName, string? bankReference);
    Task<Deposit?> GetDepositAsync(Guid id);
    Task<List<Deposit>> GetDepositsAsync(Guid? deliveryEmployeeId, DepositStatus? status);
    Task<Deposit> ReviewDepositAsync(Guid id, DepositStatus status, string? adminNotes, string? rejectionReason, string adminCode);
}

public class CollectionService : ICollectionService
{
    private readonly BakeryDbContext _context;

    public CollectionService(BakeryDbContext context) { _context = context; }

    public async Task<CashCollection> RecordCollectionAsync(Guid deliveryEmployeeId, DateTime collectionDate, decimal collectedAmount, VarianceReason varianceReason, string? varianceNotes)
    {
        if (collectedAmount < 0) throw new Exception("Collected amount cannot be negative.");

        var employee = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == deliveryEmployeeId && e.RoleType == EmployeeRoleType.Delivery && e.IsActive);
        if (employee == null) throw new Exception("Delivery employee not found or inactive.");

        var existing = await _context.CashCollections
            .FirstOrDefaultAsync(c => c.DeliveryEmployeeId == deliveryEmployeeId && c.CollectionDate.Date == collectionDate.Date);
        if (existing != null)
            throw new Exception($"Collection for {collectionDate:yyyy-MM-dd} already recorded.");

        // Expected = sum of sales recorded that day by resellers assigned to this delivery employee
        var expected = 0m; // Simple MVP: set expected = 0, admin can update later via reconciliation

        var collection = new CashCollection
        {
            DeliveryEmployeeId = deliveryEmployeeId,
            DeliveryEmployee = employee,
            CollectionDate = collectionDate,
            ExpectedAmount = expected,
            CollectedAmount = collectedAmount,
            Variance = collectedAmount - expected,
            VarianceReason = varianceReason,
            VarianceNotes = varianceNotes,
            Status = collectionDate.Date == DateTime.UtcNow.Date ? CollectionStatus.RECORDED : CollectionStatus.RECONCILED
        };

        _context.CashCollections.Add(collection);
        await _context.SaveChangesAsync();
        return collection;
    }

    public async Task<CashCollection?> GetCollectionAsync(Guid id)
        => await _context.CashCollections
            .Include(c => c.DeliveryEmployee)
            .Include(c => c.ReconciledByAdmin)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<CashCollection>> GetCollectionsAsync(Guid? deliveryEmployeeId, CollectionStatus? status)
    {
        var query = _context.CashCollections
            .Include(c => c.DeliveryEmployee)
            .AsQueryable();
        if (deliveryEmployeeId.HasValue) query = query.Where(c => c.DeliveryEmployeeId == deliveryEmployeeId.Value);
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        return await query.OrderByDescending(c => c.CollectionDate).ToListAsync();
    }

    public async Task<CashCollection> ReconcileCollectionAsync(Guid id, string? adminNotes, string adminCode)
    {
        var collection = await GetCollectionAsync(id);
        if (collection == null) throw new Exception("Collection not found.");

        var admin = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == adminCode && e.RoleType == EmployeeRoleType.Admin);
        if (admin == null) throw new Exception("Admin not found.");

        collection.Status = CollectionStatus.RECONCILED;
        collection.ReconciledAt = DateTime.UtcNow;
        collection.ReconciledByAdminId = admin.Id;
        collection.ReconciledByAdmin = admin;
        collection.AdminNotes = adminNotes;

        await _context.SaveChangesAsync();
        return collection;
    }

    public async Task<Deposit> SubmitDepositAsync(Guid deliveryEmployeeId, Guid? cashCollectionId, decimal amount, DateTime depositDate, string? bankName, string? bankReference)
    {
        if (amount <= 0) throw new Exception("Deposit amount must be positive.");

        var employee = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == deliveryEmployeeId && e.RoleType == EmployeeRoleType.Delivery && e.IsActive);
        if (employee == null) throw new Exception("Delivery employee not found or inactive.");

        // Generate reference number
        var count = await _context.Deposits.CountAsync() + 1;
        var reference = $"DEP-{DateTime.UtcNow:yyyyMMdd}-{count:D4}";

        var deposit = new Deposit
        {
            ReferenceNumber = reference,
            DeliveryEmployeeId = deliveryEmployeeId,
            DeliveryEmployee = employee,
            CashCollectionId = cashCollectionId,
            Amount = amount,
            DepositDate = depositDate,
            BankName = bankName,
            BankReference = bankReference,
            Status = DepositStatus.SUBMITTED
        };

        _context.Deposits.Add(deposit);
        await _context.SaveChangesAsync();
        return deposit;
    }

    public async Task<Deposit?> GetDepositAsync(Guid id)
        => await _context.Deposits
            .Include(d => d.DeliveryEmployee)
            .Include(d => d.CashCollection)
            .Include(d => d.ReviewedByAdmin)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<List<Deposit>> GetDepositsAsync(Guid? deliveryEmployeeId, DepositStatus? status)
    {
        var query = _context.Deposits
            .Include(d => d.DeliveryEmployee)
            .AsQueryable();
        if (deliveryEmployeeId.HasValue) query = query.Where(d => d.DeliveryEmployeeId == deliveryEmployeeId.Value);
        if (status.HasValue) query = query.Where(d => d.Status == status.Value);
        return await query.OrderByDescending(d => d.SubmittedAt).ToListAsync();
    }

    public async Task<Deposit> ReviewDepositAsync(Guid id, DepositStatus status, string? adminNotes, string? rejectionReason, string adminCode)
    {
        var deposit = await GetDepositAsync(id);
        if (deposit == null) throw new Exception("Deposit not found.");

        var admin = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == adminCode && e.RoleType == EmployeeRoleType.Admin);
        if (admin == null) throw new Exception("Admin not found.");

        deposit.Status = status;
        deposit.ReviewedAt = DateTime.UtcNow;
        deposit.ReviewedByAdminId = admin.Id;
        deposit.ReviewedByAdmin = admin;
        deposit.AdminNotes = adminNotes;
        if (status == DepositStatus.REJECTED) deposit.RejectionReason = rejectionReason;

        await _context.SaveChangesAsync();
        return deposit;
    }
}
