using Fleet.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.Api.Entities.InterventionTypes;

public static class InterventionTypeServiceConfiguration
{
    // Budget: simple (3/5 simple)
    public static EntityServiceCollection<FleetDbContext> AddInterventionTypes(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<InterventionType, int, InterventionTypeSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
