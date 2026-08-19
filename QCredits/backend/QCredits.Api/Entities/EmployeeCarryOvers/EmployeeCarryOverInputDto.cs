namespace QCredits.Api.Entities.EmployeeCarryOvers;

public class EmployeeCarryOverInputDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public decimal CarriedOverCredits { get; set; }
    public string? Note { get; set; }
}
