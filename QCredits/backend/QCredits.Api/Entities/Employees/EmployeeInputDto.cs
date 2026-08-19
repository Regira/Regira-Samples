namespace QCredits.Api.Entities.Employees;

public class EmployeeInputDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public EmployeeRole Role { get; set; } = EmployeeRole.Employee;
}
