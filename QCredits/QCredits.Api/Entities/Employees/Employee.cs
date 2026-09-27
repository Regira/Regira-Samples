using System.ComponentModel.DataAnnotations;
using QCredits.Api.Entities.Departments;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace QCredits.Api.Entities.Employees;

public class Employee : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? FirstName { get; set; }
    [Required, MaxLength(64)] public string? LastName { get; set; }
    [Required, MaxLength(256)] public string? Email { get; set; }
    [MaxLength(100)] public string? JobTitle { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(FirstName), nameof(LastName), nameof(Email), nameof(JobTitle)])]
    public string? NormalizedContent { get; set; }
}

public record EmployeeSearchObject : SearchObject
{
    public ICollection<int>? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
    public string? Email { get; set; }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? JobTitle { get; set; }
    public int DepartmentId { get; set; }
    public DepartmentDto? Department { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class EmployeeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string? FirstName { get; set; }
    [Required, MaxLength(64)] public string? LastName { get; set; }
    [Required, MaxLength(256), EmailAddress] public string? Email { get; set; }
    [MaxLength(100)] public string? JobTitle { get; set; }
    public int DepartmentId { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;
}
