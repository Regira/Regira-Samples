using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace RoomPlanner.Api.Entities.Employees;

public class Employee : IEntityWithSerial, IHasTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string FirstName { get; set; } = null!;
    [Required, MaxLength(64)] public string LastName { get; set; } = null!;
    /// <summary>Display name ("First Last"), kept in sync by the prepper.</summary>
    [MaxLength(130)] public string? Title { get; set; }
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(64)] public string? Department { get; set; }
    [MaxLength(64)] public string? JobTitle { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(1024), Normalized(SourceProperties = [nameof(FirstName), nameof(LastName), nameof(Email), nameof(Department), nameof(JobTitle)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public record EmployeeSearchObject : SearchObject
{
    public ICollection<string>? Department { get; set; }
    public bool? IsActive { get; set; }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Title { get; set; }
    public string Email { get; set; } = null!;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class EmployeeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string FirstName { get; set; } = null!;
    [Required, MaxLength(64)] public string LastName { get; set; } = null!;
    [Required, MaxLength(256), EmailAddress] public string Email { get; set; } = null!;
    [MaxLength(64)] public string? Department { get; set; }
    [MaxLength(64)] public string? JobTitle { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
