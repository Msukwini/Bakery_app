namespace BakeryApp.Api.DTOs;

public record CreateResellerDto(
    string FirstName, 
    string LastName, 
    string Email, 
    string PhoneNumber, 
    string Code, 
    Guid? ResidenceId
);

public record UpdateResellerDto(
    string FirstName, 
    string LastName, 
    string Email, 
    string PhoneNumber, 
    bool IsActive, 
    Guid? ResidenceId
);

public record ResellerResponseDto(
    Guid EmployeeId,
    Guid PersonId,
    string Code, 
    string FirstName, 
    string LastName, 
    string Email, 
    string PhoneNumber, 
    bool IsActive, 
    Guid? ResidenceId, 
    string? ResidenceName
);