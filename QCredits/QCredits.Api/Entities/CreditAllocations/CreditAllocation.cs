using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QCredits.Api.Entities.Employees;
using Regira.Entities.Attributes;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;

namespace QCredits.Api.Entities.CreditAllocations;

/// <summary>
/// The yearly QCredit budget of one employee.
/// Free credits = Annual - Reserved + CarriedOver; Remaining = Free - Used (approved requests).
/// </summary>
public class CreditAllocation : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }
    [ServerOwned] public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    [ServerOwned] public int Year { get; set; }

    public double AnnualCredits { get; set; }
    /// <summary>Credits reserved for mandatory company days.</summary>
    public double ReservedCredits { get; set; }
    /// <summary>Reserved credits already consumed by attended company days.</summary>
    public double ReservedUsed { get; set; }
    /// <summary>Credits carried over from the previous year (negative = a deficit carried over).</summary>
    public double CarriedOver { get; set; }
    /// <summary>Lowest balance this employee may reach (default -10).</summary>
    public double MinBalance { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    // filled by CreditAllocationProcessor
    [NotMapped] public double FreeCredits { get; set; }
    [NotMapped] public double UsedCredits { get; set; }
    [NotMapped] public double PendingCredits { get; set; }
    [NotMapped] public double RemainingCredits { get; set; }
}

public record CreditAllocationSearchObject : SearchObject
{
    public ICollection<int>? EmployeeId { get; set; }
    public ICollection<int>? DepartmentId { get; set; }
    public int? Year { get; set; }
}

public class CreditAllocationDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public int Year { get; set; }
    public double AnnualCredits { get; set; }
    public double ReservedCredits { get; set; }
    public double ReservedUsed { get; set; }
    public double CarriedOver { get; set; }
    public double MinBalance { get; set; }
    public string? Notes { get; set; }
    public double FreeCredits { get; set; }
    public double UsedCredits { get; set; }
    public double PendingCredits { get; set; }
    public double RemainingCredits { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CreditAllocationInputDto
{
    public int Id { get; set; }
    // EmployeeId and Year are fixed once created ([ServerOwned] restores them on update)
    public int EmployeeId { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
    [Range(0, 200)] public double AnnualCredits { get; set; }
    [Range(0, 200)] public double ReservedCredits { get; set; }
    [Range(0, 200)] public double ReservedUsed { get; set; }
    [Range(-200, 200)] public double CarriedOver { get; set; }
    [Range(-200, 0)] public double MinBalance { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
}
