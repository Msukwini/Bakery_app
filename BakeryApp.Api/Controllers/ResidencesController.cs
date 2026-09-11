using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryApp.Api.Controllers;

public class CreateResidenceDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int EstimatedPopulation { get; set; }
    public int MaxResellerCapacity { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ResidencesController : ControllerBase
{
    private readonly BakeryDbContext _context;

    public ResidencesController(BakeryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var residences = await _context.Residences
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Address,
                r.EstimatedPopulation,
                r.MaxResellerCapacity
            })
            .ToListAsync();

        return Ok(residences);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResidenceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { error = "Name is required." });

        var residence = new Residence
        {
            Name = dto.Name,
            Address = dto.Address ?? "",
            EstimatedPopulation = dto.EstimatedPopulation,
            MaxResellerCapacity = dto.MaxResellerCapacity
        };

        _context.Residences.Add(residence);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = residence.Id,
            name = residence.Name,
            address = residence.Address,
            estimatedPopulation = residence.EstimatedPopulation,
            maxResellerCapacity = residence.MaxResellerCapacity
        });
    }
}
