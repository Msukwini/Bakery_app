namespace BakeryApp.Core.Entities;

public class Residence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int EstimatedPopulation { get; set; }
    public int MaxResellerCapacity { get; set; }

    // Navigation property: Active resellers assigned to this residence
    public ICollection<EmployeeId> AssignedResellers { get; set; } = new List<EmployeeId>();
}