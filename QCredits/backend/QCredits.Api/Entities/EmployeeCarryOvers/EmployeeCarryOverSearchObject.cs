using Regira.Entities.Models;

namespace QCredits.Api.Entities.EmployeeCarryOvers;

public record EmployeeCarryOverSearchObject : SearchObject
{
    public ICollection<int>? EmployeeId { get; set; }
    public ICollection<int>? Year { get; set; }
}
