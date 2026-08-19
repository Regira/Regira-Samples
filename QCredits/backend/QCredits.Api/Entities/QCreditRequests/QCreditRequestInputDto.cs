namespace QCredits.Api.Entities.QCreditRequests;

public class QCreditRequestInputDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Year { get; set; }

    // Status/DecisionDate/ApproverId/DecisionNotes intentionally absent - they are server-owned and
    // are written only through the workflow controller (approve/reject). See QCreditRequestStatusPrimer.

    // Nullable + uninitialized: null = untouched on save, omitted collection never wipes stored items.
    public ICollection<QCreditRequestItemInputDto>? Items { get; set; }
}
