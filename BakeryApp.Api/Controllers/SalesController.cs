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
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;
    private readonly BakeryApp.Infrastructure.Data.BakeryDbContext _context;

    public SalesController(ISalesService salesService, BakeryApp.Infrastructure.Data.BakeryDbContext context)
    {
        _salesService = salesService;
        _context = context;
    }

    [HttpPost("record")]
    [Authorize(Roles = "Reseller,Admin")]
    public async Task<IActionResult> RecordSale([FromBody] RecordSaleRequest request)
    {
        // Get the current user's employee code safely
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(employeeCode))
            return Unauthorized("Employee code not found in token.");

        var currentUser = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null)
            return Unauthorized("User not found.");

        Guid resellerId;
        if (User.IsInRole("Admin") && request.ResellerEmployeeId.HasValue)
        {
            resellerId = request.ResellerEmployeeId.Value;
        }
        else
        {
            if (currentUser.RoleType != EmployeeRoleType.Reseller)
                return BadRequest("Only resellers can record sales without an explicit reseller ID.");
            resellerId = currentUser.Id;
        }

        try
        {
            var sale = await _salesService.RecordSaleAsync(
                resellerId,
                request.ProductVariantId,
                request.Quantity,
                request.UnitPriceAtSale,
                currentUser.Id
            );

            var ledgerEntries = await _context.CommissionLedgerEntries
                .Where(e => e.ResellerSaleId == sale.Id)
                .ToListAsync();
            var totalCommission = ledgerEntries.Sum(e => e.AmountEarned);

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

            return Ok(new SaleResponse
            {
                SaleId = sale.Id,
                ProductName = variant?.Product?.Name ?? "Unknown",
                VariantName = variant?.SizeName ?? "Unknown",
                Quantity = sale.Quantity,
                UnitPrice = sale.UnitPriceAtSale,
                TotalAmount = sale.Quantity * sale.UnitPriceAtSale,
                CommissionEarned = totalCommission,
                SaleDate = sale.SaleDate
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("commission/{resellerEmployeeId}")]
    [Authorize(Roles = "Admin,Reseller")]
    public async Task<IActionResult> GetCommissionLedger(Guid resellerEmployeeId)
    {
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(employeeCode))
            return Unauthorized();

        var currentUser = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null) return Unauthorized();

        if (currentUser.RoleType == EmployeeRoleType.Reseller && currentUser.Id != resellerEmployeeId)
            return Forbid("You can only view your own commission ledger.");

        var entries = await _salesService.GetCommissionLedgerAsync(resellerEmployeeId);

        var response = entries.Select(e => new CommissionLedgerResponse
        {
            Id = e.Id,
            ProductName = e.ResellerSale?.ProductVariant?.Product?.Name ?? "Unknown",
            VariantName = e.ResellerSale?.ProductVariant?.SizeName ?? "Unknown",
            Quantity = e.ResellerSale?.Quantity ?? 0,
            UnitPrice = e.ResellerSale?.UnitPriceAtSale ?? 0,
            AmountEarned = e.AmountEarned,
            AmountPaid = e.AmountPaid,
            IsSettled = e.IsSettled,
            CreatedAt = e.CreatedAt,
            SaleId = e.ResellerSaleId
        });

        return Ok(response);
    }

    [HttpGet("balance/{resellerEmployeeId}")]
    [Authorize(Roles = "Admin,Reseller")]
    public async Task<IActionResult> GetCommissionBalance(Guid resellerEmployeeId)
    {
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;
        if (string.IsNullOrEmpty(employeeCode))
            return Unauthorized();

        var currentUser = await _context.EmployeeIds
            .FirstOrDefaultAsync(e => e.Code == employeeCode);
        if (currentUser == null) return Unauthorized();

        if (currentUser.RoleType == EmployeeRoleType.Reseller && currentUser.Id != resellerEmployeeId)
            return Forbid("You can only view your own balance.");

        var entries = await _salesService.GetCommissionLedgerAsync(resellerEmployeeId);
        var totalEarned = entries.Sum(e => e.AmountEarned);
        var totalPaid = entries.Sum(e => e.AmountPaid);
        var outstanding = totalEarned - totalPaid;

        return Ok(new CommissionBalanceResponse
        {
            Outstanding = outstanding,
            TotalEarned = totalEarned,
            TotalPaid = totalPaid
        });
    }
}
