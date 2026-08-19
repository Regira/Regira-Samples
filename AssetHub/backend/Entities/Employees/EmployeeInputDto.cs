using System.ComponentModel.DataAnnotations;

namespace AssetHub.Api.Entities.Employees;

public class EmployeeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(80)]
    public string FirstName { get; set; } = null!;
    [Required, MaxLength(80)]
    public string LastName { get; set; } = null!;
    [Required, MaxLength(150)]
    public string Email { get; set; } = null!;
    [MaxLength(100)]
    public string? Department { get; set; }
    [MaxLength(100)]
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
}
