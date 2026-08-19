using QCredits.Api.Entities.Employees;

namespace QCredits.Api.Entities.EmployeeCarryOvers;

public class EmployeeCarryOverDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public int Year { get; set; }
    public decimal CarriedOverCredits { get; set; }
    public string? Note { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
