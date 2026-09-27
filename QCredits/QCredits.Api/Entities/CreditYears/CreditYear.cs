using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace QCredits.Api.Entities.CreditYears;

/// <summary>
/// The credit policy of one calendar year: the defaults new allocations get.
/// </summary>
public class CreditYear : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
    /// <summary>Total annual budget per employee (default 20).</summary>
    [Range(0, 200)] public double AnnualCredits { get; set; } = 20;
    /// <summary>Part of the annual budget reserved for mandatory company days (default 5).</summary>
    [Range(0, 200)] public double ReservedCredits { get; set; } = 5;
    /// <summary>Maximum number of remaining credits that may carry over to the next year (default 10).</summary>
    [Range(0, 200)] public double MaxCarryOver { get; set; } = 10;
    /// <summary>Lowest balance an employee may reach (default -10).</summary>
    [Range(-200, 0)] public double MinBalance { get; set; } = -10;
    /// <summary>A closed year accepts no new requests; its balances have been rolled over.</summary>
    public bool IsClosed { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CreditYearDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public double AnnualCredits { get; set; }
    public double ReservedCredits { get; set; }
    public double MaxCarryOver { get; set; }
    public double MinBalance { get; set; }
    public bool IsClosed { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CreditYearInputDto
{
    public int Id { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
    [Range(0, 200)] public double AnnualCredits { get; set; } = 20;
    [Range(0, 200)] public double ReservedCredits { get; set; } = 5;
    [Range(0, 200)] public double MaxCarryOver { get; set; } = 10;
    [Range(-200, 0)] public double MinBalance { get; set; } = -10;
    public bool IsClosed { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
}
