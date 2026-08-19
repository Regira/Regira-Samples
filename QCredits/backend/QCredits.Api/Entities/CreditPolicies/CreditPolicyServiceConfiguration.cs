using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using QCredits.Api.Data;

namespace QCredits.Api.Entities.CreditPolicies;

public static class CreditPolicyServiceConfiguration
{
    public static EntityServiceCollection<QCreditsDbContext> AddCreditPolicies(this IEntityServiceCollection<QCreditsDbContext> services)
        => services.For<CreditPolicy, int, CreditPolicySearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.Year?.Any() == true)
                {
                    query = query.Where(x => so.Year.Contains(x.Year));
                }
                return query;
            });
            e.SortBy(query => query.OrderByDescending(x => x.Year));
        });
}
