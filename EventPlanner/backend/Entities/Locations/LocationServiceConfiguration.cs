using EventPlanner.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.Locations;

public static class LocationServiceConfiguration
{
    // Simple registration — 1/5 simple budget slot
    public static EntityServiceCollection<EventPlannerDbContext> AddLocations(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Location>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
