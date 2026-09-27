using QCredits.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace QCredits.Api.Entities.CreditYears;

public static class CreditYearServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddCreditYears(this IEntityServiceCollection<AppDbContext> services)
        => services.For<CreditYear>(e =>
        {
            // CreditYear has no text to search: ?q= matches the year number
            e.Filter((query, so) => so != null && int.TryParse(so.Q, out var year) ? query.Where(x => x.Year == year) : query);
            e.SortBy(query => query.OrderByDescending(x => x.Year));
            e.Prepare(item =>
            {
                if (item.ReservedCredits > item.AnnualCredits)
                    throw new EntityInputException<CreditYear>("Invalid credit policy")
                    {
                        InputErrors = { [nameof(CreditYear.ReservedCredits)] = "Reserved credits cannot exceed the annual budget." }
                    };
            });
        });
}
