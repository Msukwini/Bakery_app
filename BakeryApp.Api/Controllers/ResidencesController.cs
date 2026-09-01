using BakeryApp.Api.DTOs;
using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResidencesController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public ResidencesController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResidenceResponseDto>>> GetAll()
    {
        var residences = await _context.Residences
            .Select(r => new ResidenceResponseDto(
                r.Id,
                r.Name,
                r.Address,
                r.EstimatedPopulation,
                r.MaxResellerCapacity,
                r.AssignedResellers.Count
            ))
            .ToListAsync();

        return Ok(residences);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResidenceResponseDto>> GetById(Guid id)
    {
        var residence = await _context.Residences
            .Where(r => r.Id == id)
            .Select(r => new ResidenceResponseDto(
                r.Id,
                r.Name,
                r.Address,
                r.EstimatedPopulation,
                r.MaxResellerCapacity,
                r.AssignedResellers.Count
            ))
            .FirstOrDefaultAsync();

        if (residence == null) return NotFound();

        return Ok(residence);
    }

    [HttpPost]
    public async Task<ActionResult<ResidenceResponseDto>> Create(CreateResidenceDto dto)
    {
        var residence = new Residence
        {
            Name = dto.Name,
            Address = dto.Address,
            EstimatedPopulation = dto.EstimatedPopulation,
            MaxResellerCapacity = dto.MaxResellerCapacity
        };

        _context.Residences.Add(residence);
        await _context.SaveChangesAsync();

        var response = new ResidenceResponseDto(
            residence.Id,
            residence.Name,
            residence.Address,
            residence.EstimatedPopulation,
            residence.MaxResellerCapacity,
            0
        );

        return CreatedAtAction(nameof(GetById), new { id = residence.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateResidenceDto dto)
    {
        var residence = await _context.Residences.FindAsync(id);
        if (residence == null) return NotFound();

        residence.Name = dto.Name;
        residence.Address = dto.Address;
        residence.EstimatedPopulation = dto.EstimatedPopulation;
        residence.MaxResellerCapacity = dto.MaxResellerCapacity;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var residence = await _context.Residences.FindAsync(id);
        if (residence == null) return NotFound();

        _context.Residences.Remove(residence);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}