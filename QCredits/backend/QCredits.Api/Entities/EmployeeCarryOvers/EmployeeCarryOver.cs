using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using QCredits.Api.Entities.Employees;

namespace QCredits.Api.Entities.EmployeeCarryOvers;

/// <summary>
/// Admin-managed carry-over of unused QCredits from one year into the next (capped by CreditPolicy.MaxCarryOver).
/// </summary>
public class EmployeeCarryOver : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    /// <summary>The year the credits are carried INTO.</summary>
    public int Year { get; set; }

    public decimal CarriedOverCredits { get; set; }

    [MaxLength(256)]
    public string? Note { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
