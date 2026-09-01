namespace BakeryApp.Api.DTOs;

public record CreateResidenceDto(
    string Name, 
    string Address, 
    int EstimatedPopulation, 
    int MaxResellerCapacity
);

public record UpdateResidenceDto(
    string Name, 
    string Address, 
    int EstimatedPopulation, 
    int MaxResellerCapacity
);

public record ResidenceResponseDto(
    Guid Id, 
    string Name, 
    string Address, 
    int EstimatedPopulation, 
    int MaxResellerCapacity, 
    int AssignedResellerCount
);