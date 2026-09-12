namespace BakeryApp.Core.Entities;

public class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Profile
    public string? ProfilePicturePath { get; set; }
    public string? UniversityName { get; set; }
    public string? StudentEmail { get; set; }

    // Password setup
    public string? PasswordSetupToken { get; set; }
    public DateTime? PasswordSetupTokenExpiry { get; set; }
    public bool HasSetPassword { get; set; } = false;

    public ICollection<EmployeeId> EmployeeIds { get; set; } = new List<EmployeeId>();
}
