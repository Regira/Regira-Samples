using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using QCredits.Api.Entities.Employees;

namespace QCredits.Api.Entities.QCreditRequests;

/// <summary>
/// An employee's request to spend QCredits on one or more training purchases/activities. A single
/// request can bundle several line items and requires admin approval before credits are deducted.
/// </summary>
public class QCreditRequest : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    /// <summary>The budget year these credits are spent from.</summary>
    public int Year { get; set; }

    // Status/decision fields are server-owned (kept off the input DTO) and are written only through
    // QCreditRequestWorkflowController (approve/reject) - see QCreditRequestStatusPrimer.
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTime SubmittedDate { get; set; }
    public DateTime? DecisionDate { get; set; }

    public int? ApproverId { get; set; }
    public Employee? Approver { get; set; }

    [MaxLength(1024)]
    public string? DecisionNotes { get; set; }

    /// <summary>Sum of Items[].Credits, recomputed server-side on every save.</summary>
    public decimal TotalCredits { get; set; }

    public ICollection<QCreditRequestItem>? Items { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
