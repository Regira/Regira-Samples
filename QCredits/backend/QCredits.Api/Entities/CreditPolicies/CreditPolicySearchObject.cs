using Regira.Entities.Models;

namespace QCredits.Api.Entities.CreditPolicies;

public record CreditPolicySearchObject : SearchObject
{
    public ICollection<int>? Year { get; set; }
}
