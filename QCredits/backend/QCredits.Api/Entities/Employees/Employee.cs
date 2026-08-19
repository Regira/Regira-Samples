using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace QCredits.Api.Entities.Employees;

public enum EmployeeRole
{
    Employee = 0,
    Admin = 1
}

public class Employee : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(64)]
    public string LastName { get; set; } = null!;

    [Required, MaxLength(128)]
    public string Email { get; set; } = null!;

    [MaxLength(64)]
    public string? Department { get; set; }

    [MaxLength(64)]
    public string? JobTitle { get; set; }

    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public EmployeeRole Role { get; set; } = EmployeeRole.Employee;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(FirstName), nameof(LastName), nameof(Email), nameof(Department)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
