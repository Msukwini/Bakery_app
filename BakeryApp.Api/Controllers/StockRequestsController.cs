using BakeryApp.Api.DTOs;
using BakeryApp.Core.Enums;
using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockRequestsController : ControllerBase
{
    private readonly IStockRequestService _service;
    private readonly BakeryApp.Infrastructure.Data.BakeryDbContext _context;

    public StockRequestsController(IStockRequestService service, BakeryApp.Infrastructure.Data.BakeryDbContext context)
    {
        _service = service;
        _context = context;
    }

    private async Task<Guid?> GetCurrentEmployeeIdAsync(EmployeeRoleType role)
    {
        var code = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(code)) return null;
        var emp = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == code && e.RoleType == role);
        return emp?.Id;
    }

    [HttpPost]
    [Authorize(Roles = "Reseller")]
    public async Task<IActionResult> Create([FromBody] CreateStockRequestDto dto)
    {
        var id = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Reseller);
        if (id == null) return BadRequest("Reseller account not found.");
        try
        {
            var req = await _service.CreateRequestAsync(id.Value, dto.ProductVariantId, dto.RequestedQuantity, dto.ResellerNotes);
            return Ok(new { id = req.Id, status = req.Status.ToString() });
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] StockRequestStatus? status)
    {
        List<Core.Entities.ResellerStockRequest> requests;
        if (User.IsInRole("Admin"))
            requests = await _service.GetRequestsAsync(null, status);
        else if (User.IsInRole("Reseller"))
        {
            var id = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Reseller);
            if (id == null) return Forbid();
            requests = await _service.GetRequestsAsync(id.Value, status);
        }
        else return Forbid();

        return Ok(requests.Select(MapToDto));
    }

    [HttpGet("my-deliveries")]
    [Authorize(Roles = "Delivery")]
    public async Task<IActionResult> MyDeliveries()
    {
        var id = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Delivery);
        if (id == null) return BadRequest("Delivery employee not found.");
        var list = await _service.GetDeliveriesForDriverAsync(id.Value);
        return Ok(list.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var req = await _service.GetRequestAsync(id);
        if (req == null) return NotFound();

        if (User.IsInRole("Reseller"))
        {
            var resellerId = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Reseller);
            if (resellerId != req.ResellerEmployeeId) return Forbid();
        }
        else if (User.IsInRole("Delivery"))
        {
            var driverId = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Delivery);
            if (driverId != req.AssignedDeliveryEmployeeId && driverId != req.ActualDeliveryEmployeeId) return Forbid();
        }

        return Ok(MapToDto(req));
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewStockRequestDto dto)
    {
        if (!dto.AllocatedQuantity.HasValue || dto.AllocatedQuantity.Value <= 0)
            return BadRequest("AllocatedQuantity is required.");

        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(adminCode)) return Unauthorized();

        try
        {
            var req = await _service.ApproveRequestAsync(id, dto.AllocatedQuantity.Value, dto.AssignedDeliveryEmployeeId, dto.ExpectedDeliveryDate, dto.AdminNotes, adminCode);
            return Ok(MapToDto(req));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReviewStockRequestDto dto)
    {
        if (string.IsNullOrEmpty(dto.RejectionReason))
            return BadRequest("RejectionReason is required.");

        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(adminCode)) return Unauthorized();

        try
        {
            var req = await _service.RejectRequestAsync(id, dto.RejectionReason, adminCode);
            return Ok(MapToDto(req));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}/deliver")]
    [Authorize(Roles = "Delivery")]
    public async Task<IActionResult> MarkDelivered(Guid id, [FromBody] MarkDeliveredDto dto)
    {
        var driverId = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Delivery);
        if (driverId == null) return BadRequest("Delivery employee not found.");
        try
        {
            var req = await _service.MarkDeliveredAsync(id, driverId.Value, dto.DeliveredQuantity, dto.DeliveryNotes);
            return Ok(MapToDto(req));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}/receive")]
    [Authorize(Roles = "Reseller")]
    public async Task<IActionResult> ConfirmReceipt(Guid id, [FromBody] ConfirmReceiptDto dto)
    {
        var resellerId = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Reseller);
        if (resellerId == null) return BadRequest("Reseller not found.");
        try
        {
            var req = await _service.ConfirmReceiptAsync(id, resellerId.Value, dto.ReceivedQuantity, dto.ReceiptNotes);
            return Ok(MapToDto(req));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("{id}/resolve-variance")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResolveVariance(Guid id, [FromBody] ResolveVarianceDto dto)
    {
        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(adminCode)) return Unauthorized();
        try
        {
            var req = await _service.ResolveVarianceAsync(id, dto.ResolutionNotes, dto.WriteOff, adminCode);
            return Ok(MapToDto(req));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("accountability/{resellerEmployeeId}")]
    [Authorize(Roles = "Admin,Reseller")]
    public async Task<IActionResult> GetAccountability(Guid resellerEmployeeId)
    {
        if (!User.IsInRole("Admin"))
        {
            var id = await GetCurrentEmployeeIdAsync(EmployeeRoleType.Reseller);
            if (id != resellerEmployeeId) return Forbid();
        }
        try
        {
            var result = await _service.GetResellerAccountabilityAsync(resellerEmployeeId);
            return Ok(result);
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    private static StockRequestResponseDto MapToDto(Core.Entities.ResellerStockRequest r) => new()
    {
        Id = r.Id,
        ResellerCode = r.Reseller?.Code ?? "Unknown",
        ProductName = r.ProductVariant?.Product?.Name ?? "Unknown",
        VariantName = r.ProductVariant?.SizeName ?? "Unknown",
        RequestedQuantity = r.RequestedQuantity,
        AllocatedQuantity = r.AllocatedQuantity,
        DeliveredQuantity = r.DeliveredQuantity,
        ReceivedQuantity = r.ReceivedQuantity,
        Status = r.Status,
        RequestedAt = r.RequestedAt,
        ReviewedAt = r.ReviewedAt,
        AllocatedAt = r.AllocatedAt,
        ExpectedDeliveryDate = r.ExpectedDeliveryDate,
        DeliveryCompletedAt = r.DeliveryCompletedAt,
        ReceivedAt = r.ReceivedAt,
        AssignedDeliveryCode = r.AssignedDeliveryEmployee?.Code,
        AssignedDeliveryName = r.AssignedDeliveryEmployee?.Person != null
            ? $"{r.AssignedDeliveryEmployee.Person.FirstName} {r.AssignedDeliveryEmployee.Person.LastName}"
            : null,
        ActualDeliveryCode = r.ActualDeliveryEmployee?.Code,
        ActualDeliveryName = r.ActualDeliveryEmployee?.Person != null
            ? $"{r.ActualDeliveryEmployee.Person.FirstName} {r.ActualDeliveryEmployee.Person.LastName}"
            : null,
        AdminNotes = r.AdminNotes,
        RejectionReason = r.RejectionReason,
        ResellerNotes = r.ResellerNotes,
        DeliveryNotes = r.DeliveryNotes,
        ReceiptNotes = r.ReceiptNotes,
        Variance = r.Variance,
        VarianceStatus = r.VarianceStatus,
        VarianceNotes = r.VarianceNotes
    };
}
