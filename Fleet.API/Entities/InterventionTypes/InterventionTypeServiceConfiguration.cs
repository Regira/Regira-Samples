using Fleet.API.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.API.Entities.InterventionTypes;

public static class InterventionTypeServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddInterventionTypes(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<InterventionType, int, InterventionTypeSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Code))
                    query = query.Where(x => x.Code == so.Code);
                return query;
            });
        });
}
