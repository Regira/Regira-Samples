using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace QCredits.Api.Entities.QCreditRequests;

/// <summary>
/// One purchase/activity line on a QCreditRequest - owned by the parent request via e.Related(), no
/// own .For&lt;&gt;() registration and no controller.
/// </summary>
public class QCreditRequestItem : IEntityWithSerial
{
    public int Id { get; set; }

    public int RequestId { get; set; }
    public QCreditRequest? Request { get; set; }

    [Required, MaxLength(256)]
    public string Description { get; set; } = null!;

    public CreditActivityType Type { get; set; }

    /// <summary>Number of QCredits this activity costs (1 QCredit = half a working day / EUR 250).</summary>
    public decimal Credits { get; set; }

    public DateTime ActivityDate { get; set; }

    /// <summary>Optional real-world cost in EUR, for reference.</summary>
    public decimal? Cost { get; set; }

    [MaxLength(128)]
    public string? Provider { get; set; }
}
