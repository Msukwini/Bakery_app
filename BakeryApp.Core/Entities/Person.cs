namespace BakeryApp.Core.Entities;

public class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: One person can hold multiple distinct Employee IDs
    public ICollection<EmployeeId> EmployeeIds { get; set; } = new List<EmployeeId>();
}