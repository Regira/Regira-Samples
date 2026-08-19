using QCredits.Api.Entities.Employees;

namespace QCredits.Api.Entities.QCreditRequests;

public class QCreditRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public int Year { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime SubmittedDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public int? ApproverId { get; set; }
    public EmployeeDto? Approver { get; set; }
    public string? DecisionNotes { get; set; }
    public decimal TotalCredits { get; set; }
    public ICollection<QCreditRequestItemDto>? Items { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
