namespace QCredits.Api.Entities.Employees;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
    public EmployeeRole Role { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
