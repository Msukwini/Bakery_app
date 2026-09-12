namespace BakeryApp.Api.DTOs;

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? UniversityName { get; set; }
    public string? StudentEmail { get; set; }
}

public class ProfileResponseDto
{
    public Guid PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? UniversityName { get; set; }
    public string? StudentEmail { get; set; }
    public bool HasProfilePicture { get; set; }
    public List<RoleInfo> Roles { get; set; } = new();
}

public class RoleInfo
{
    public string Code { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class PublicProfileDto
{
    public Guid PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool HasProfilePicture { get; set; }
}
