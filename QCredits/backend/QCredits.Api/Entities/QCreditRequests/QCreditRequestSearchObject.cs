using Regira.Entities.Models;

namespace QCredits.Api.Entities.QCreditRequests;

public record QCreditRequestSearchObject : SearchObject
{
    public ICollection<int>? EmployeeId { get; set; }
    public ICollection<int>? Year { get; set; }
    public ICollection<RequestStatus>? Status { get; set; }
}
