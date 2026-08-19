namespace EventPlanner.Api.Entities.Employees;

public class EmployeeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
