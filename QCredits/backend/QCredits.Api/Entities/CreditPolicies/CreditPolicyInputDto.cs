namespace QCredits.Api.Entities.CreditPolicies;

public class CreditPolicyInputDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public decimal AnnualCredits { get; set; } = 20m;
    public decimal ReservedCredits { get; set; } = 5m;
    public decimal MaxCarryOver { get; set; } = 10m;
    public decimal MinBalance { get; set; } = -10m;
}
