using BakeryApp.Api.DTOs;
using BakeryApp.Core.Entities;
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
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _service;
    private readonly BakeryApp.Infrastructure.Data.BakeryDbContext _context;

    public CollectionsController(ICollectionService service, BakeryApp.Infrastructure.Data.BakeryDbContext context)
    {
        _service = service;
        _context = context;
    }

    private async Task<Guid?> GetCurrentDeliveryEmployeeIdAsync()
    {
        var code = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(code)) return null;
        var emp = await _context.EmployeeIds.FirstOrDefaultAsync(e => e.Code == code && e.RoleType == EmployeeRoleType.Delivery);
        return emp?.Id;
    }

    [HttpPost]
    [Authorize(Roles = "Delivery")]
    public async Task<IActionResult> Record([FromBody] RecordCollectionDto dto)
    {
        var id = await GetCurrentDeliveryEmployeeIdAsync();
        if (id == null) return BadRequest("Delivery employee account not found.");
        try
        {
            var c = await _service.RecordCollectionAsync(id.Value, dto.CollectionDate, dto.CollectedAmount, dto.VarianceReason, dto.VarianceNotes);
            return Ok(MapCollection(c));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] CollectionStatus? status)
    {
        List<CashCollection> list;
        if (User.IsInRole("Admin"))
            list = await _service.GetCollectionsAsync(null, status);
        else
        {
            var id = await GetCurrentDeliveryEmployeeIdAsync();
            if (id == null) return Forbid();
            list = await _service.GetCollectionsAsync(id.Value, status);
        }
        return Ok(list.Select(MapCollection));
    }

    [HttpPut("{id}/reconcile")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reconcile(Guid id, [FromBody] ReconcileCollectionDto dto)
    {
        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(adminCode)) return Unauthorized();
        try
        {
            var c = await _service.ReconcileCollectionAsync(id, dto.AdminNotes, adminCode);
            return Ok(MapCollection(c));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("deposit")]
    [Authorize(Roles = "Delivery")]
    public async Task<IActionResult> SubmitDeposit([FromBody] SubmitDepositDto dto)
    {
        var id = await GetCurrentDeliveryEmployeeIdAsync();
        if (id == null) return BadRequest("Delivery employee account not found.");
        try
        {
            var d = await _service.SubmitDepositAsync(id.Value, dto.CashCollectionId, dto.Amount, dto.DepositDate, dto.BankName, dto.BankReference);
            return Ok(MapDeposit(d));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("deposits")]
    public async Task<IActionResult> ListDeposits([FromQuery] DepositStatus? status)
    {
        List<Deposit> list;
        if (User.IsInRole("Admin"))
            list = await _service.GetDepositsAsync(null, status);
        else
        {
            var id = await GetCurrentDeliveryEmployeeIdAsync();
            if (id == null) return Forbid();
            list = await _service.GetDepositsAsync(id.Value, status);
        }
        return Ok(list.Select(MapDeposit));
    }

    [HttpGet("deposits/{id}")]
    public async Task<IActionResult> GetDeposit(Guid id)
    {
        var d = await _service.GetDepositAsync(id);
        if (d == null) return NotFound();
        if (!User.IsInRole("Admin"))
        {
            var empId = await GetCurrentDeliveryEmployeeIdAsync();
            if (empId != d.DeliveryEmployeeId) return Forbid();
        }
        return Ok(MapDeposit(d));
    }

    [HttpPut("deposits/{id}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReviewDeposit(Guid id, [FromBody] ReviewDepositDto dto)
    {
        var adminCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(adminCode)) return Unauthorized();
        try
        {
            var d = await _service.ReviewDepositAsync(id, dto.Status, dto.AdminNotes, dto.RejectionReason, adminCode);
            return Ok(MapDeposit(d));
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    private static CollectionResponseDto MapCollection(CashCollection c) => new()
    {
        Id = c.Id,
        DeliveryEmployeeCode = c.DeliveryEmployee?.Code ?? "Unknown",
        CollectionDate = c.CollectionDate,
        ExpectedAmount = c.ExpectedAmount,
        CollectedAmount = c.CollectedAmount,
        Variance = c.Variance,
        VarianceReason = c.VarianceReason,
        Status = c.Status,
        RecordedAt = c.RecordedAt,
        ReconciledAt = c.ReconciledAt,
        AdminNotes = c.AdminNotes
    };

    private static DepositResponseDto MapDeposit(Deposit d) => new()
    {
        Id = d.Id,
        ReferenceNumber = d.ReferenceNumber,
        DeliveryEmployeeCode = d.DeliveryEmployee?.Code ?? "Unknown",
        CashCollectionId = d.CashCollectionId,
        Amount = d.Amount,
        DepositDate = d.DepositDate,
        BankName = d.BankName,
        BankReference = d.BankReference,
        Status = d.Status,
        SubmittedAt = d.SubmittedAt,
        ReviewedAt = d.ReviewedAt,
        AdminNotes = d.AdminNotes,
        RejectionReason = d.RejectionReason
    };
}
