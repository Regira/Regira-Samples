using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;

namespace QCredits.Api.Entities.CreditPolicies;

/// <summary>
/// Company-wide QCredit policy for a given calendar year, managed by administrators.
/// </summary>
public class CreditPolicy : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }
    public int Year { get; set; }

    /// <summary>Total annual QCredits granted to every employee (default 20).</summary>
    public decimal AnnualCredits { get; set; } = 20m;

    /// <summary>Credits reserved for mandatory company training days (default 5).</summary>
    public decimal ReservedCredits { get; set; } = 5m;

    /// <summary>Maximum number of unused credits an employee may carry over to the next year (default 10).</summary>
    public decimal MaxCarryOver { get; set; } = 10m;

    /// <summary>Lowest balance an employee is allowed to reach once a request is approved (default -10).</summary>
    public decimal MinBalance { get; set; } = -10m;

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    [NotMapped]
    public decimal FreelyAvailableCredits => AnnualCredits - ReservedCredits;
}
