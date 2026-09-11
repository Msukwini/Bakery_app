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

    private async Task<Guid?> GetCurrentResellerIdAsync()
    {
        var code = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(code)) return null;
        var emp = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == code && e.RoleType == EmployeeRoleType.Reseller);
        return emp?.Id;
    }

    [HttpPost]
    [Authorize(Roles = "Reseller")]
    public async Task<IActionResult> Create([FromBody] CreateStockRequestDto dto)
    {
        var resellerId = await GetCurrentResellerIdAsync();
        if (resellerId == null) return BadRequest("Reseller account not found.");

        try
        {
            var req = await _service.CreateRequestAsync(resellerId.Value, dto.ProductVariantId, dto.RequestedQuantity, dto.ResellerNotes);
            return Ok(new { id = req.Id, status = req.Status.ToString() });
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] StockRequestStatus? status)
    {
        List<Core.Entities.ResellerStockRequest> requests;
        if (User.IsInRole("Admin"))
        {
            requests = await _service.GetRequestsAsync(null, status);
        }
        else
        {
            var resellerId = await GetCurrentResellerIdAsync();
            if (resellerId == null) return Forbid();
            requests = await _service.GetRequestsAsync(resellerId.Value, status);
        }

        return Ok(requests.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var req = await _service.GetRequestAsync(id);
        if (req == null) return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var resellerId = await GetCurrentResellerIdAsync();
            if (resellerId == null || req.ResellerEmployeeId != resellerId.Value)
                return Forbid();
        }
        return Ok(MapToDto(req));
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReviewStockRequestDto dto)
    {
        if (!dto.AllocatedQuantity.HasValue || dto.AllocatedQuantity.Value <= 0)
            return BadRequest("AllocatedQuantity is required for approval.");

        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(adminCode)) return Unauthorized();

        try
        {
            var req = await _service.ApproveRequestAsync(id, dto.AllocatedQuantity.Value, dto.AdminNotes, adminCode);
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

    [HttpPut("{id}/receive")]
    [Authorize(Roles = "Reseller")]
    public async Task<IActionResult> ConfirmReceipt(Guid id)
    {
        var resellerId = await GetCurrentResellerIdAsync();
        if (resellerId == null) return BadRequest("Reseller account not found.");

        try
        {
            var req = await _service.ConfirmReceiptAsync(id, resellerId.Value);
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
            var resellerId = await GetCurrentResellerIdAsync();
            if (resellerId != resellerEmployeeId) return Forbid();
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
        Status = r.Status,
        RequestedAt = r.RequestedAt,
        ReviewedAt = r.ReviewedAt,
        AllocatedAt = r.AllocatedAt,
        ReceivedAt = r.ReceivedAt,
        AdminNotes = r.AdminNotes,
        RejectionReason = r.RejectionReason,
        ResellerNotes = r.ResellerNotes
    };
}
