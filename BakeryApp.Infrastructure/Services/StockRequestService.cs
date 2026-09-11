using BakeryApp.Core.Entities;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Infrastructure.Services;

public interface IStockRequestService
{
    Task<ResellerStockRequest> CreateRequestAsync(Guid resellerEmployeeId, Guid productVariantId, int requestedQuantity, string? resellerNotes);
    Task<ResellerStockRequest?> GetRequestAsync(Guid requestId);
    Task<List<ResellerStockRequest>> GetRequestsAsync(Guid? resellerEmployeeId, StockRequestStatus? status);
    Task<ResellerStockRequest> ApproveRequestAsync(Guid requestId, int allocatedQuantity, string? adminNotes, string adminEmployeeCode);
    Task<ResellerStockRequest> RejectRequestAsync(Guid requestId, string rejectionReason, string adminEmployeeCode);
    Task<ResellerStockRequest> ConfirmReceiptAsync(Guid requestId, Guid resellerEmployeeId);
    Task<ResellerStockAccountabilityDtoResult> GetResellerAccountabilityAsync(Guid resellerEmployeeId);
}

public class ResellerStockAccountabilityDtoResult
{
    public Guid ResellerEmployeeId { get; set; }
    public string ResellerCode { get; set; } = string.Empty;
    public int TotalReceived { get; set; }
    public int TotalSold { get; set; }
    public int TotalDamaged { get; set; }
    public int TotalReturned { get; set; }
    public int ExpectedClosingStock { get; set; }
    public List<ResellerStockItemResult> Items { get; set; } = new();
}

public class ResellerStockItemResult
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Sold { get; set; }
    public int Damaged { get; set; }
    public int Returned { get; set; }
    public int ExpectedStock { get; set; }
}

public class StockRequestService : IStockRequestService
{
    private readonly BakeryDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;

    public StockRequestService(BakeryDbContext context, IInventoryService inventoryService, INotificationService notificationService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
    }

    public async Task<ResellerStockRequest> CreateRequestAsync(Guid resellerEmployeeId, Guid productVariantId, int requestedQuantity, string? resellerNotes)
    {
        if (requestedQuantity <= 0) throw new Exception("Requested quantity must be positive.");

        var reseller = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller && e.IsActive);
        if (reseller == null) throw new Exception("Reseller not found or inactive.");

        var variant = await _context.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == productVariantId);
        if (variant == null) throw new Exception("Product variant not found.");

        var request = new ResellerStockRequest
        {
            ResellerEmployeeId = resellerEmployeeId,
            Reseller = reseller,
            ProductVariantId = productVariantId,
            ProductVariant = variant,
            RequestedQuantity = requestedQuantity,
            ResellerNotes = resellerNotes,
            Status = StockRequestStatus.PENDING,
            RequestedAt = DateTime.UtcNow
        };

        _context.ResellerStockRequests.Add(request);
        await _context.SaveChangesAsync();
        await _notificationService.NotifyStockRequestSubmittedAsync(request);
        return request;
    }

    public async Task<ResellerStockRequest?> GetRequestAsync(Guid requestId)
    {
        return await _context.ResellerStockRequests
            .Include(r => r.Reseller)
            .Include(r => r.ProductVariant).ThenInclude(v => v.Product)
            .Include(r => r.ReviewedByAdmin)
            .FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public async Task<List<ResellerStockRequest>> GetRequestsAsync(Guid? resellerEmployeeId, StockRequestStatus? status)
    {
        var query = _context.ResellerStockRequests
            .Include(r => r.Reseller)
            .Include(r => r.ProductVariant).ThenInclude(v => v.Product)
            .AsQueryable();

        if (resellerEmployeeId.HasValue) query = query.Where(r => r.ResellerEmployeeId == resellerEmployeeId.Value);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        return await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
    }

    public async Task<ResellerStockRequest> ApproveRequestAsync(Guid requestId, int allocatedQuantity, string? adminNotes, string adminEmployeeCode)
    {
        if (allocatedQuantity <= 0) throw new Exception("Allocated quantity must be positive.");

        var request = await GetRequestAsync(requestId);
        if (request == null) throw new Exception("Request not found.");
        if (request.Status != StockRequestStatus.PENDING)
            throw new Exception($"Request is already {request.Status}.");

        var admin = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == adminEmployeeCode && e.RoleType == EmployeeRoleType.Admin);
        if (admin == null) throw new Exception("Admin not found.");

        // Check available stock
        var currentStock = await _inventoryService.GetCurrentStockAsync(request.ProductVariantId);
        if (currentStock < allocatedQuantity)
            throw new Exception($"Insufficient stock. Available: {currentStock}, Requested: {allocatedQuantity}");

        // Deduct from main inventory and record allocation
        var ledgerEntry = await _inventoryService.DeductStockAsync(
            request.ProductVariantId,
            allocatedQuantity,
            InventoryTransactionType.AllocatedToReseller,
            $"Stock allocation to {request.Reseller.Code} - Request {request.Id}",
            request.ResellerEmployeeId
        );

        request.AllocatedQuantity = allocatedQuantity;
        request.Status = StockRequestStatus.ALLOCATED;
        request.ReviewedAt = DateTime.UtcNow;
        request.AllocatedAt = DateTime.UtcNow;
        request.ReviewedByAdminId = admin.Id;
        request.AdminNotes = adminNotes;
        request.InventoryLedgerEntryId = ledgerEntry.Id;

        await _context.SaveChangesAsync();
        await _notificationService.NotifyStockRequestApprovedAsync(request);
        return request;
    }

    public async Task<ResellerStockRequest> RejectRequestAsync(Guid requestId, string rejectionReason, string adminEmployeeCode)
    {
        var request = await GetRequestAsync(requestId);
        if (request == null) throw new Exception("Request not found.");
        if (request.Status != StockRequestStatus.PENDING)
            throw new Exception($"Request is already {request.Status}.");

        var admin = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == adminEmployeeCode && e.RoleType == EmployeeRoleType.Admin);
        if (admin == null) throw new Exception("Admin not found.");

        request.Status = StockRequestStatus.REJECTED;
        request.RejectionReason = rejectionReason;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedByAdminId = admin.Id;

        await _context.SaveChangesAsync();
        await _notificationService.NotifyStockRequestRejectedAsync(request);
        return request;
    }

    public async Task<ResellerStockRequest> ConfirmReceiptAsync(Guid requestId, Guid resellerEmployeeId)
    {
        var request = await GetRequestAsync(requestId);
        if (request == null) throw new Exception("Request not found.");
        if (request.ResellerEmployeeId != resellerEmployeeId)
            throw new Exception("This request does not belong to you.");
        if (request.Status != StockRequestStatus.ALLOCATED)
            throw new Exception($"Cannot confirm receipt. Status is {request.Status}.");

        request.Status = StockRequestStatus.RECEIVED;
        request.ReceivedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<ResellerStockAccountabilityDtoResult> GetResellerAccountabilityAsync(Guid resellerEmployeeId)
    {
        var reseller = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Id == resellerEmployeeId && e.RoleType == EmployeeRoleType.Reseller);
        if (reseller == null) throw new Exception("Reseller not found.");

        var entries = await _context.InventoryLedgerEntries
            .Include(e => e.ProductVariant).ThenInclude(v => v.Product)
            .Where(e => e.EmployeeId == resellerEmployeeId)
            .ToListAsync();

        var grouped = entries
            .GroupBy(e => e.ProductVariantId)
            .Select(g => new ResellerStockItemResult
            {
                ProductVariantId = g.Key,
                ProductName = g.First().ProductVariant?.Product?.Name ?? "Unknown",
                VariantName = g.First().ProductVariant?.SizeName ?? "Unknown",
                Received = -g.Where(x => x.TransactionType == InventoryTransactionType.AllocatedToReseller).Sum(x => x.Quantity),
                Sold = -g.Where(x => x.TransactionType == InventoryTransactionType.SaleConfirmed).Sum(x => x.Quantity),
                Damaged = -g.Where(x => x.TransactionType == InventoryTransactionType.DamagedOrReturned && x.Quantity < 0).Sum(x => x.Quantity),
                Returned = g.Where(x => x.TransactionType == InventoryTransactionType.DamagedOrReturned && x.Quantity > 0).Sum(x => x.Quantity)
            })
            .ToList();

        foreach (var item in grouped)
        {
            item.ExpectedStock = item.Received - item.Sold - item.Damaged + item.Returned;
        }

        return new ResellerStockAccountabilityDtoResult
        {
            ResellerEmployeeId = resellerEmployeeId,
            ResellerCode = reseller.Code,
            TotalReceived = grouped.Sum(i => i.Received),
            TotalSold = grouped.Sum(i => i.Sold),
            TotalDamaged = grouped.Sum(i => i.Damaged),
            TotalReturned = grouped.Sum(i => i.Returned),
            ExpectedClosingStock = grouped.Sum(i => i.ExpectedStock),
            Items = grouped
        };
    }
}
