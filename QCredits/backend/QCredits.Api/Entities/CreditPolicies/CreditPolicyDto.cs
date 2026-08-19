namespace QCredits.Api.Entities.CreditPolicies;

public class CreditPolicyDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public decimal AnnualCredits { get; set; }
    public decimal ReservedCredits { get; set; }
    public decimal MaxCarryOver { get; set; }
    public decimal MinBalance { get; set; }
    public decimal FreelyAvailableCredits { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
