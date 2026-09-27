using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.Locations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace AssetHub.Api.Entities.Employees;

// Not IArchivable: assignment history rows point at employees through a required FK.
// Leavers are flagged with IsActive = false instead.
public class Employee : IEntityWithSerial, IHasCode, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    /// <summary>Employee number (unique)</summary>
    [Required, MaxLength(16)] public string? Code { get; set; }
    [Required, MaxLength(64)] public string? FirstName { get; set; }
    [Required, MaxLength(64)] public string? LastName { get; set; }
    [Required, MaxLength(128), EmailAddress] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    [MaxLength(64)] public string? Department { get; set; }
    [MaxLength(64)] public string? JobTitle { get; set; }
    public int? LocationId { get; set; }
    public Location? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly? HireDate { get; set; }

    /// <summary>Read-only history; written only by the asset assign / return actions</summary>
    public ICollection<AssetAssignment>? Assignments { get; set; }

    [NotMapped] public int? CurrentAssetCount { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Code), nameof(FirstName), nameof(LastName), nameof(Email), nameof(Department)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public record EmployeeSearchObject : SearchObject
{
    public ICollection<int>? LocationId { get; set; }
    public ICollection<string>? Department { get; set; }
    public bool? IsActive { get; set; }
    /// <summary>true = currently holds at least one asset</summary>
    public bool? HasAssets { get; set; }
}

public enum EmployeeSortBy
{
    Default = 0,
    LastName,
    LastNameDesc,
    Code,
    CodeDesc,
    Department,
    DepartmentDesc,
    HireDate,
    HireDateDesc,
    Created,
    CreatedDesc
}

[Flags]
public enum EmployeeIncludes
{
    Default = 0,
    Assignments = 1 << 0,
    All = Assignments
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public int? LocationId { get; set; }
    public LocationDto? Location { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? HireDate { get; set; }
    public int? CurrentAssetCount { get; set; }
    public ICollection<EmployeeAssignmentDto>? Assignments { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>Assignment as seen from the employee (carries the asset instead of the employee)</summary>
public class EmployeeAssignmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public AssetCoreDto? Asset { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AssignedOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
    public string? Notes { get; set; }
    public string? ReturnNotes { get; set; }
}

public class EmployeeInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(16)] public string? Code { get; set; }
    [Required, MaxLength(64)] public string? FirstName { get; set; }
    [Required, MaxLength(64)] public string? LastName { get; set; }
    [Required, MaxLength(128), EmailAddress] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    [MaxLength(64)] public string? Department { get; set; }
    [MaxLength(64)] public string? JobTitle { get; set; }
    public int? LocationId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly? HireDate { get; set; }
    // Assignments deliberately absent: the asset workflow is its only writer
}
