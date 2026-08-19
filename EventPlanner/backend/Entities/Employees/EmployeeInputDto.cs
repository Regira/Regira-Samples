using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Api.Entities.Employees;

public class EmployeeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(128)] public string? Department { get; set; }
    [MaxLength(128)] public string? JobTitle { get; set; }
    [MaxLength(1024)] public string? AvatarUrl { get; set; }
}
