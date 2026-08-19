namespace QCredits.Api.Balances;

public class EmployeeBalanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string? Department { get; set; }
    public int Year { get; set; }
    public decimal AnnualCredits { get; set; }
    public decimal ReservedCredits { get; set; }
    public decimal FreelyAvailableCredits { get; set; }
    public decimal CarriedOverCredits { get; set; }
    public decimal ApprovedCredits { get; set; }
    public decimal PendingCredits { get; set; }
    public decimal RemainingCredits { get; set; }
    public decimal MinBalance { get; set; }
}
